// ─── File Download ─────────────────────────────────────────────────────────
window.downloadFileFromBytes = function (fileName, contentType, bytesBase64) {
    const bytes = Uint8Array.from(atob(bytesBase64), c => c.charCodeAt(0));
    const blob = new Blob([bytes], { type: contentType });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
};

// ─── View File in Browser Tab ───────────────────────────────────────────────
window.viewFileInBrowser = function (contentType, bytesBase64) {
    const bytes = Uint8Array.from(atob(bytesBase64), c => c.charCodeAt(0));
    const blob = new Blob([bytes], { type: contentType });
    const url = URL.createObjectURL(blob);
    window.open(url, '_blank');
};

// ─── Print HTML in New Window ────────────────────────────────────────────────
window.printHtml = function (html, title) {
    var win = window.open('', '_blank', 'width=900,height=700');
    if (!win) {
        alert('Popup blocked. Please allow popups for this site to use the print feature.');
        return;
    }
    win.document.write(`<!DOCTYPE html><html><head>
        <title>${title}</title>
        <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css">
        <style>
            body { font-family: Arial, sans-serif; padding: 24px; font-size: 14px; }
            .timeline-item { margin-bottom: 1rem; padding-bottom: 1rem; border-bottom: 1px solid #dee2e6; }
            @media print { body { padding: 8px; } }
        </style>
    </head><body>${html}
    <script>setTimeout(function(){ window.print(); }, 600);<\/script>
    </body></html>`);
    win.document.close();
};
