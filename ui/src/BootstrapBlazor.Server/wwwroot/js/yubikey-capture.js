// Catching what a YubiKey types.
//
// The key is a keyboard: touching it emits 44 characters in roughly half a second, then usually
// Enter. Two things follow from that, and both are the whole reason this file exists.
//
// 1. Capture happens here, not in Blazor. Routing 44 keystrokes through @onkeydown means 44 SignalR
//    round trips during a burst measured in milliseconds — the character counter would crawl behind
//    the user's finger. One interop call at the end costs one round trip.
//
// 2. We read event.code, never event.key. A Vietnamese input method rewrites characters as they
//    arrive: Telex folds "dd" into "đ", and modhex also contains e/i/u alongside the tone keys
//    f/j/r, so "ef" becomes "è" and so on — ten distinct ways for a valid OTP to arrive corrupted,
//    with no way to undo it afterwards because several inputs collapse onto the same output.
//    event.code is the physical key, decided before any input method sees it.
//
// Measured on a real key, 2026-08-22: touching it into an ordinary text box with Unikey running
// produced 43 characters containing "đ"; the same touch read through event.code produced all 44.

const MODHEX = 'cbdefghijklnrtuv';
const OTP_LENGTH = 44;
const IDLE_MS = 400;        // the key types continuously; a gap this long means it finished
const PROGRESS_MS = 120;    // how often the counter is allowed to cost a round trip

let session = null;

export function arm(dotNetRef, timeoutSeconds) {
    disarm();

    session = {
        dotNetRef,
        buffer: '',
        idleTimer: null,
        progressTimer: null,
        lastReported: -1,
        deadline: setTimeout(() => finish('timeout'), (timeoutSeconds || 60) * 1000),
        onKeyDown: null
    };

    session.onKeyDown = (event) => {
        if (!session) return;

        if (event.key === 'Enter') {
            event.preventDefault();
            finish('done');
            return;
        }

        const physical = /^Key([A-Z])$/.exec(event.code || '');
        if (!physical) return;

        const ch = physical[1].toLowerCase();
        if (!MODHEX.includes(ch)) return;

        // Stop the characters reaching whatever has focus. Without this the OTP lands in the
        // page's inputs, or in the browser's find bar, in plain sight.
        event.preventDefault();
        session.buffer += ch;

        scheduleProgress();

        clearTimeout(session.idleTimer);
        if (session.buffer.length >= OTP_LENGTH) {
            finish('done');
            return;
        }
        session.idleTimer = setTimeout(() => finish('done'), IDLE_MS);
    };

    document.addEventListener('keydown', session.onKeyDown, true);
    return true;
}

export function disarm() {
    if (!session) return;
    document.removeEventListener('keydown', session.onKeyDown, true);
    clearTimeout(session.idleTimer);
    clearTimeout(session.deadline);
    clearTimeout(session.progressTimer);
    session = null;
}

// Throttled: a live counter is worth one round trip every eighth of a second, not one per keystroke.
function scheduleProgress() {
    if (!session || session.progressTimer) return;
    session.progressTimer = setTimeout(() => {
        if (!session) return;
        session.progressTimer = null;
        if (session.buffer.length !== session.lastReported) {
            session.lastReported = session.buffer.length;
            session.dotNetRef.invokeMethodAsync('OnProgress', session.buffer.length).catch(() => { });
        }
    }, PROGRESS_MS);
}

function finish(reason) {
    if (!session) return;

    const { dotNetRef, buffer } = session;
    disarm();

    if (reason === 'timeout') {
        dotNetRef.invokeMethodAsync('OnTimedOut').catch(() => { });
        return;
    }

    dotNetRef.invokeMethodAsync('OnKeystrokesCaptured', buffer).catch(() => { });
}
