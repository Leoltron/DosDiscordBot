import { debounce } from "debounce";

export default function initControls($field, nextEvent, prevEvent) {
    let autoFwdStatus = 0; // 0 - stop, 1 - 1000ms, 2 - 400ms, 3 - 200 ms
    const $pauseBtn = $field.find('.controls__pause');
    const $1stepBtn = $field.find('.controls__step-fwd');
    const $stepBackBtn = $field.find('.controls__step-back');
    const $auto500Btn = $field.find('.controls__auto-fwd-1000');
    const $auto200Btn = $field.find('.controls__auto-fwd-400');
    const $auto100Btn = $field.find('.controls__auto-fwd-200');

    setupEvents();
    setInterval(() => is1000ms() && nextEvent(), 1000);
    setInterval(() => is400ms() && nextEvent(), 400);
    setInterval(() => is200ms() && nextEvent(), 200);

    function setupEvents() {
        $1stepBtn.click(debounce(nextEvent, 100, true));
        $1stepBtn.click(pause);
        $stepBackBtn.click(debounce(prevEvent, 100, true));
        $stepBackBtn.click(pause);
        $pauseBtn.click(pause);
        $auto500Btn.click(() => setAutoFwdStatus(1));
        $auto200Btn.click(() => setAutoFwdStatus(2));
        $auto100Btn.click(() => setAutoFwdStatus(3));
    }


    function isPaused() {
        return autoFwdStatus === 0;
    }

    function is1000ms() {
        return autoFwdStatus === 1;
    }

    function is400ms() {
        return autoFwdStatus === 2;
    }

    function is200ms() {
        return autoFwdStatus === 3;
    }
    
    function pause(){
        setAutoFwdStatus(0);
    }

    function setAutoFwdStatus(newStatus) {
        autoFwdStatus = newStatus;
        updateSwitches();
    }

    function updateSwitches() {
        setSwitchStatus($pauseBtn, isPaused());
        setSwitchStatus($auto500Btn, is1000ms());
        setSwitchStatus($auto200Btn, is400ms());
        setSwitchStatus($auto100Btn, is200ms());
    }

    function setSwitchStatus($switch, isActive) {
        if (isActive)
            $switch.addClass('controls__control_active');
        else
            $switch.removeClass('controls__control_active');
    }
}