window.quillInterop = {
    // Initialiseer Quill editor en bewaar referentie
    init: function (elementId, initialContent, dotNetRef) {
        const quill = new Quill(`#${elementId}`, { theme: 'snow' });
        quill.root.innerHTML = initialContent || '';

        // Text-change event: stuur content terug naar C#
        if (dotNetRef) {
            quill.on('text-change', (delta, oldDelta, source) => {
                const html = quill.root.innerHTML;
                dotNetRef.invokeMethodAsync('OnQuillContentChanged', html);
            });
        }

        // bewaar referentie
        window.QuillFunctions = window.QuillFunctions || {};
        window.QuillFunctions._instances = window.QuillFunctions._instances || {};
        window.QuillFunctions._instances[elementId] = quill;

        return quill;
    },

    // Haal HTML op uit Quill
    getContent: function (elementId) {
        const quill = window.QuillFunctions._instances?.[elementId];
        return quill ? quill.root.innerHTML : null;
    },

    // Zet HTML in Quill
    setContent: function (elementId, html) {
        const quill = window.QuillFunctions._instances?.[elementId];
        if (quill) {
            quill.root.innerHTML = html || '';
        }
    }
};
