export function measure(element) {
    const rect = element.getBoundingClientRect();
    return {
        width: Math.round(rect.width),
        height: Math.round(rect.height),
        language: navigator.language
    };
}
