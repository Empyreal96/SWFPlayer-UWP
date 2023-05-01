var keyboardEvent = document.createEvent('KeyboardEvent');
var initMethod = typeof keyboardEvent.initKeyboardEvent !== 'undefined' ? 'initKeyboardEvent' : 'initKeyEvent';

keyboardEvent[initMethod](
    'keydown', /* event type: keydown, keyup, keypress */
    true, /* bubbles */
    true, /* cancelable */
    window, /* view: should be window */
    false, /* ctrlKey */
    false, /* altKey */
    false, /* shiftKey */
    false, /* metaKey */
    40, /* keyCode: unsigned long - the virtual key code, else 0 */
    0, /* charCode: unsigned long - the Unicode character associated with the depressed key, else 0 */ ); document.dispatchEvent(keyboardEvent);