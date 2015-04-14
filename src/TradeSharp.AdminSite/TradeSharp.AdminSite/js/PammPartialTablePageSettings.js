function SetSettings(prorertyName, prorertyValue) {
    if (!IsSessionStorageAvailable()) return;
    try {
        window.sessionStorage.setItem(prorertyName, prorertyValue);
    }
    catch (e) {}
}

function GetSettings(prorertyName) {
    if (!IsSessionStorageAvailable()) return null;
    try {
        return window.sessionStorage.getItem(prorertyName);
    } catch (e) {
        return null;
    }
}

function IsLocalStorageAvailable() {
    try {
        return ('localStorage' in window) && window['localStorage'] && window.localStorage !== null;
    } catch (e) {
        return false;
    }
}

function IsSessionStorageAvailable() {
    try {
        return ('sessionStorage' in window) && window['sessionStorage'] && window.sessionStorage !== null;
    } catch (e) {
        return false;
    }
}