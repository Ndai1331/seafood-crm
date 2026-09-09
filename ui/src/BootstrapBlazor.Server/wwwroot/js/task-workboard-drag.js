const bindings = new WeakMap();
const dragThreshold = 12;

function closestWithin(root, target, selector) {
    if (!(target instanceof Element)) return null;
    const element = target.closest(selector);
    return element && root.contains(element) ? element : null;
}

function laneAtPoint(root, clientX, clientY) {
    const target = document.elementFromPoint(clientX, clientY);
    return closestWithin(root, target, ".task-workboard-lane[data-drop-status]");
}

function clearDropTarget(binding) {
    binding.dropTarget?.classList.remove("is-drop-target");
    binding.dropTarget = null;
}

function releasePointerCapture(root, pointerId) {
    if (pointerId != null && root.hasPointerCapture?.(pointerId)) {
        root.releasePointerCapture(pointerId);
    }
}

function clearPointerState(binding) {
    const pointerId = binding.pointerId;
    binding.card?.classList.remove("is-dragging");
    binding.root.classList.remove("is-pointer-dragging");
    binding.pointerId = null;
    binding.card = null;
    binding.itemId = null;
    binding.sourceStatus = null;
    binding.startX = 0;
    binding.startY = 0;
    binding.dragging = false;
    clearDropTarget(binding);
    releasePointerCapture(binding.root, pointerId);
}

function setDropTarget(binding, lane) {
    if (binding.dropTarget === lane) return;
    clearDropTarget(binding);
    binding.dropTarget = lane;
    lane?.classList.add("is-drop-target");
}

function completeDrop(binding, clientX, clientY) {
    const lane = laneAtPoint(binding.root, clientX, clientY);
    const itemId = binding.itemId;
    const sourceStatus = binding.sourceStatus;
    const targetStatus = lane?.dataset.dropStatus;
    clearPointerState(binding);

    binding.suppressClickItemId = itemId ? String(itemId) : null;
    window.setTimeout(() => {
        if (binding.suppressClickItemId === String(itemId)) binding.suppressClickItemId = null;
    }, 0);
    if (!itemId || !targetStatus || sourceStatus === targetStatus) return;
    void binding.dotNetRef.invokeMethodAsync("HandleCardDropAsync", itemId, targetStatus)
        .catch(error => console.error("Task workboard drop failed", error));
}

export function init(root, dotNetRef) {
    if (!root) return;

    const existing = bindings.get(root);
    if (existing) {
        existing.dotNetRef = dotNetRef;
        return;
    }

    const binding = {
        root,
        dotNetRef,
        pointerId: null,
        card: null,
        itemId: null,
        sourceStatus: null,
        startX: 0,
        startY: 0,
        dragging: false,
        dropTarget: null,
        suppressClickItemId: null
    };

    binding.onPointerDown = event => {
        if (!event.isPrimary || event.button !== 0 || event.pointerType === "touch") return;
        const dragSurface = closestWithin(root, event.target, ".task-workboard-card-topline");
        if (!dragSurface) return;
        const card = closestWithin(root, dragSurface, ".task-workboard-card[data-work-item-id]");
        if (!card || card.dataset.dragEnabled !== "true") return;

        const itemId = Number(card.dataset.workItemId);
        const sourceStatus = card.dataset.workItemStatus;
        if (!Number.isSafeInteger(itemId) || itemId <= 0 || !sourceStatus) return;

        if (binding.pointerId != null) clearPointerState(binding);
        binding.pointerId = event.pointerId;
        binding.card = card;
        binding.itemId = itemId;
        binding.sourceStatus = sourceStatus;
        binding.startX = event.clientX;
        binding.startY = event.clientY;
        binding.dragging = false;
    };

    binding.onPointerMove = event => {
        if (binding.pointerId !== event.pointerId || !binding.card) return;

        if (!binding.dragging) {
            const distance = Math.hypot(event.clientX - binding.startX, event.clientY - binding.startY);
            if (distance < dragThreshold) return;
            binding.dragging = true;
            root.setPointerCapture?.(event.pointerId);
            binding.card.classList.add("is-dragging");
            root.classList.add("is-pointer-dragging");
        }

        event.preventDefault();
        setDropTarget(binding, laneAtPoint(root, event.clientX, event.clientY));
    };

    binding.onPointerUp = event => {
        if (binding.pointerId !== event.pointerId) return;
        if (binding.dragging) {
            event.preventDefault();
            completeDrop(binding, event.clientX, event.clientY);
        } else {
            clearPointerState(binding);
        }
    };

    binding.onPointerCancel = event => {
        if (binding.pointerId !== event.pointerId) return;
        clearPointerState(binding);
    };

    binding.onLostPointerCapture = event => {
        if (binding.pointerId !== event.pointerId) return;
        clearPointerState(binding);
    };

    binding.onClick = event => {
        if (!binding.suppressClickItemId) return;
        const clickedCard = closestWithin(root, event.target, ".task-workboard-card[data-work-item-id]");
        if (clickedCard?.dataset.workItemId !== binding.suppressClickItemId) return;
        binding.suppressClickItemId = null;
        event.preventDefault();
        event.stopPropagation();
    };

    root.addEventListener("pointerdown", binding.onPointerDown);
    root.addEventListener("pointermove", binding.onPointerMove);
    root.addEventListener("pointerup", binding.onPointerUp);
    root.addEventListener("pointercancel", binding.onPointerCancel);
    root.addEventListener("lostpointercapture", binding.onLostPointerCapture);
    root.addEventListener("click", binding.onClick, true);
    root.setAttribute("data-drag-drop-ready", "true");
    bindings.set(root, binding);
}

export function dispose(root) {
    const binding = root ? bindings.get(root) : null;
    if (!binding) return;

    clearPointerState(binding);
    root.removeEventListener("pointerdown", binding.onPointerDown);
    root.removeEventListener("pointermove", binding.onPointerMove);
    root.removeEventListener("pointerup", binding.onPointerUp);
    root.removeEventListener("pointercancel", binding.onPointerCancel);
    root.removeEventListener("lostpointercapture", binding.onLostPointerCapture);
    root.removeEventListener("click", binding.onClick, true);
    root.removeAttribute("data-drag-drop-ready");
    bindings.delete(root);
}
