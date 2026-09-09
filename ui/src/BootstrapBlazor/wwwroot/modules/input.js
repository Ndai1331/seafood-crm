import EventHandler from "./event-handler.js"

export default {
  composition(el, callback) {
    if (el) {
      const trigger = () => callback(el.value)
      // Realtime search during IME (Telex/VNI): compositionupdate fires while composing;
      // input fires for normal typing and when composition commits.
      EventHandler.on(el, 'input', trigger)
      EventHandler.on(el, 'compositionupdate', trigger)
    }
  },

  dispose(el) {
    if (el) {
      EventHandler.off(el, 'input')
      EventHandler.off(el, 'compositionupdate')
    }
  }
}
