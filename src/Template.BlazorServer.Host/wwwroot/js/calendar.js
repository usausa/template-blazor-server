export function timeZone() {
    return Intl.DateTimeFormat().resolvedOptions().timeZone;
}

export function load(key) {
    try {
        return localStorage.getItem(key);
    } catch {
        return null;
    }
}

export function save(key, value) {
    try {
        localStorage.setItem(key, value);
    } catch {
        return;
    }
}
