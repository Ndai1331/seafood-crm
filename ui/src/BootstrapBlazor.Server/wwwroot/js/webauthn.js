// WebAuthn bridge for the Blazor login and profile screens.
//
// navigator.credentials only exists in the browser, so the ceremonies have to run here and the
// results travel back to C# as JSON. Two things this file is responsible for:
//
//   1. base64url <-> ArrayBuffer. The server speaks base64url; the browser API speaks buffers.
//      Every conversion goes through the two helpers below — spreading them around is the
//      single easiest way to end up with a challenge that silently never matches.
//
//   2. Keeping the DOM error NAME. An exception thrown across interop arrives in C# as a string
//      with the name lost, and the name is the only thing that distinguishes "user did not touch
//      the key" from "wrong domain". So failures are returned as data, never thrown.

// Field names below are the WebAuthn wire names and are what Fido2NetLib binds to. They are
// matched case-sensitively on the server, so clientDataJSON must keep its capital JSON — getting
// this wrong makes every ceremony fail deserialization with a generic "unreadable" error.

const b64ToBuf = (value) => {
    const padded = value.replace(/-/g, '+').replace(/_/g, '/');
    const binary = atob(padded + '==='.slice((padded.length + 3) % 4));
    const bytes = new Uint8Array(binary.length);
    for (let i = 0; i < binary.length; i++) {
        bytes[i] = binary.charCodeAt(i);
    }
    return bytes.buffer;
};

const bufToB64 = (buffer) => {
    const bytes = new Uint8Array(buffer);
    let binary = '';
    for (let i = 0; i < bytes.length; i++) {
        binary += String.fromCharCode(bytes[i]);
    }
    return btoa(binary).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');
};

export function isSupported() {
    return typeof window.PublicKeyCredential !== 'undefined' && !!navigator.credentials;
}

const failure = (error) => ({ ok: false, error: error.name || 'UnknownError', message: error.message || '' });

// navigator.credentials can stay pending indefinitely: the spec timeout is a hint the browser may
// ignore, and some environments (a backgrounded tab, an embedded webview, a profile with no
// authenticator surface) simply never resolve it. The caller is a Blazor interop await, so a
// promise that never settles leaves the user staring at a disabled button with no way out.
// Always settling means the UI can always recover.
const withDeadline = (promise, ms) => Promise.race([
    promise,
    new Promise(resolve => setTimeout(
        () => resolve({ ok: false, error: 'CeremonyTimeout', message: `no response after ${ms}ms` }), ms))
]);

const deadlineFor = (options) => (options.timeout && options.timeout > 0 ? options.timeout : 45000) + 5000;

// Fido2 4.x emits two WebAuthn L3 fields we never asked for, both empty: `hints` and
// `attestationFormats`. An empty `hints` is a plausible way to tell a browser "offer no
// transports", and an unrecognised field is a plausible way to make one give up quietly. Neither
// carries meaning for us, so they are stripped rather than left to be interpreted.
const dropEmptyL3Fields = (options) => {
    for (const field of ['hints', 'attestationFormats']) {
        if (Array.isArray(options[field]) && options[field].length === 0) {
            delete options[field];
        }
    }
    return options;
};

export async function createCredential(optionsJson) {
    try {
        const options = dropEmptyL3Fields(JSON.parse(optionsJson));
        options.challenge = b64ToBuf(options.challenge);
        options.user.id = b64ToBuf(options.user.id);
        if (options.excludeCredentials) {
            options.excludeCredentials = options.excludeCredentials.map(c => ({ ...c, id: b64ToBuf(c.id) }));
        }

        const credential = await withDeadline(
            navigator.credentials.create({ publicKey: options }), deadlineFor(options));

        if (credential && credential.ok === false) {
            return credential;
        }

        return {
            ok: true,
            json: JSON.stringify({
                id: credential.id,
                rawId: bufToB64(credential.rawId),
                type: credential.type,
                clientExtensionResults: credential.getClientExtensionResults(),
                response: {
                    attestationObject: bufToB64(credential.response.attestationObject),
                    clientDataJSON: bufToB64(credential.response.clientDataJSON)
                }
            })
        };
    } catch (error) {
        return failure(error);
    }
}

export async function getAssertion(optionsJson) {
    try {
        const options = dropEmptyL3Fields(JSON.parse(optionsJson));
        options.challenge = b64ToBuf(options.challenge);
        if (options.allowCredentials) {
            options.allowCredentials = options.allowCredentials.map(c => ({ ...c, id: b64ToBuf(c.id) }));
        }

        const assertion = await withDeadline(
            navigator.credentials.get({ publicKey: options }), deadlineFor(options));

        if (assertion && assertion.ok === false) {
            return assertion;
        }

        return {
            ok: true,
            json: JSON.stringify({
                id: assertion.id,
                rawId: bufToB64(assertion.rawId),
                type: assertion.type,
                clientExtensionResults: assertion.getClientExtensionResults(),
                response: {
                    authenticatorData: bufToB64(assertion.response.authenticatorData),
                    clientDataJSON: bufToB64(assertion.response.clientDataJSON),
                    signature: bufToB64(assertion.response.signature),
                    userHandle: assertion.response.userHandle ? bufToB64(assertion.response.userHandle) : null
                }
            })
        };
    } catch (error) {
        return failure(error);
    }
}


// ---------------------------------------------------------------------------
// Gesture-bound ceremonies
//
// navigator.credentials only answers when it is called inside a real user
// gesture. Blazor Server handles a click on the server and calls back into JS
// over SignalR, and by then the gesture is gone: the call then hangs FOREVER —
// no dialog, no rejection, and the spec timeout is ignored. Measured on
// test.task9.pro: without activation the promise never settles; from inside a
// trusted click it answers in 4ms.
//
// So the whole ceremony runs here, started by a native click listener. The
// server is reached through the same-origin proxy because this origin is all
// the browser can address.
// ---------------------------------------------------------------------------

const BUILD = '2026-08-22-no-empty-l3';
const log = (...args) => console.log('[webauthn]', ...args);

const postJson = async (path, body, bearer) => {
    const headers = { 'Content-Type': 'application/json' };
    if (bearer) headers['Authorization'] = `Bearer ${bearer}`;

    const res = await fetch(`/webauthn-proxy/${path}`, {
        method: 'POST',
        headers,
        body: JSON.stringify(body ?? {})
    });

    const text = await res.text();
    let parsed = null;
    try { parsed = text ? JSON.parse(text) : null; } catch { /* not json */ }

    if (!res.ok) {
        const message = (parsed && (parsed.error || parsed.message)) || text || `HTTP ${res.status}`;
        const err = new Error(message);
        err.name = 'ApiError';
        throw err;
    }

    return parsed;
};

const readBearer = () => {
    // Same key the Blazor client stores its session under.
    const raw = window.localStorage.getItem('my-access-token');
    if (!raw) return null;
    try { return JSON.parse(raw); } catch { return raw; }
};

async function runCeremony(kind, temporaryToken) {
    log('ceremony start', { build: BUILD, kind, activation: navigator.userActivation?.isActive });
    const bearer = kind === 'register' ? readBearer() : null;
    if (kind === 'register') log('bearer from localStorage:', bearer ? 'present' : 'MISSING');

    const beginPath = kind === 'login' ? 'login/begin'
        : kind === 'enroll' ? 'enroll/begin'
        : 'register/begin';

    const beginBody = kind === 'register' ? {} : { temporaryToken };
    log('calling', beginPath);
    const begun = await postJson(beginPath, beginBody, bearer);
    log('options received; activation now:', navigator.userActivation?.isActive);

    if (!begun || !begun.optionsJson) {
        throw Object.assign(new Error('Máy chủ không trả về dữ liệu ceremony.'), { name: 'ApiError' });
    }

    log('options from server (before empty-L3 cleanup):', JSON.parse(begun.optionsJson));
    log('calling navigator.credentials — a prompt should appear now');
    const browserResult = kind === 'login'
        ? await getAssertion(begun.optionsJson)
        : await createCredential(begun.optionsJson);
    log('navigator.credentials returned', browserResult.ok ? 'ok' : browserResult.error);

    if (!browserResult.ok) {
        return browserResult;
    }

    if (kind === 'login') {
        const token = await postJson('login/complete',
            { temporaryToken, assertionJson: browserResult.json }, null);
        return { ok: true, token };
    }

    if (kind === 'enroll') {
        const enrolled = await postJson('enroll/complete',
            { enrollToken: temporaryToken, attestationJson: browserResult.json, deviceName: '' }, null);
        return { ok: true, token: enrolled ? enrolled.token : null };
    }

    await postJson('register/complete',
        { attestationJson: browserResult.json, deviceName: '' }, bearer);
    return { ok: true };
}

/// Attaches the ceremony to a button so it runs inside the click itself.
/// Returns false when the element is not on the page yet, so the caller can retry after a render.
export function bindCeremony(buttonId, kind, temporaryToken, dotNetRef) {
    const button = document.getElementById(buttonId);
    if (!button) return false;

    // Blazor re-renders; binding twice would run the ceremony twice per click.
    if (button.dataset.webauthnBound === kind) return true;
    button.dataset.webauthnBound = kind;

    log('button bound', { build: BUILD, buttonId, kind });

    button.addEventListener('click', async () => {
        log('click received');
        if (button.dataset.webauthnRunning === '1') return;
        button.dataset.webauthnRunning = '1';

        let result;
        try {
            result = await runCeremony(kind, temporaryToken);
        } catch (error) {
            log('ceremony failed', error);
            result = failure(error);
        } finally {
            button.dataset.webauthnRunning = '0';
        }

        try {
            await dotNetRef.invokeMethodAsync('OnWebAuthnCeremonyFinished', result);
        } catch (error) {
            console.error('[webauthn] could not report the ceremony result', error);
        }
    });

    return true;
}
