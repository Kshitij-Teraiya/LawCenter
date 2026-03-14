// JS Isolation module for InvoicePrintView.razor
// Loaded lazily when the component first renders — guaranteed to be available when PrintAsync fires.

export function printInvoice(elementId) {
    var el = document.getElementById(elementId);
    if (!el) {
        console.warn('printInvoice: element not found:', elementId);
        return;
    }

    var invoiceStyles =
        '.invoice-print-body{padding:2rem 2.5rem;font-family:Inter,Arial,sans-serif;}' +
        '.inv-header{display:flex;justify-content:space-between;align-items:flex-start;gap:2rem;}' +
        '.inv-logo{max-height:60px;max-width:160px;object-fit:contain;margin-bottom:.5rem;display:block;}' +
        '.inv-firm-name{font-size:1.1rem;font-weight:700;color:#1a1a2e;}' +
        '.inv-firm-address{font-size:.82rem;color:#555;margin-top:.2rem;}' +
        '.inv-firm-contact{font-size:.78rem;color:#666;margin-top:.2rem;}' +
        '.inv-gst{font-size:.78rem;color:#666;margin-top:.2rem;}' +
        '.inv-meta{text-align:right;flex-shrink:0;}' +
        '.inv-title{font-size:2rem;font-weight:800;letter-spacing:4px;color:#1a1a2e;}' +
        '.inv-meta-table{font-size:.82rem;margin-top:.5rem;border-collapse:collapse;margin-left:auto;}' +
        '.inv-meta-table td{padding:.1rem .4rem;}' +
        '.inv-meta-table td:first-child{color:#888;}' +
        '.inv-status-badge{padding:.15rem .5rem;border-radius:.25rem;font-size:.72rem;font-weight:600;}' +
        '.inv-status-paid{background:#d1fae5;color:#065f46;}' +
        '.inv-status-accepted{background:#dbeafe;color:#1d4ed8;}' +
        '.inv-status-pending{background:#fef9c3;color:#854d0e;}' +
        '.inv-status-rejected{background:#fee2e2;color:#991b1b;}' +
        '.inv-divider{border-color:#1a1a2e;margin:1.25rem 0;}' +
        '.inv-bill-section{display:flex;gap:3rem;margin-bottom:1.5rem;}' +
        '.inv-section-label{font-size:.68rem;font-weight:700;letter-spacing:2px;color:#888;text-transform:uppercase;margin-bottom:.25rem;}' +
        '.inv-client-name,.inv-lawyer-name{font-weight:600;font-size:1rem;}' +
        '.inv-table{width:100%;border-collapse:collapse;font-size:.88rem;}' +
        '.inv-table th{background:#1a1a2e;color:white;padding:.5rem .75rem;font-weight:600;font-size:.8rem;letter-spacing:.5px;}' +
        '.inv-table td{padding:.6rem .75rem;border-bottom:1px solid #f0f0f0;}' +
        '.inv-table tr:last-child td{border-bottom:none;}' +
        '.inv-summary{display:flex;justify-content:flex-end;margin-top:1rem;}' +
        '.inv-summary-table{min-width:240px;border-collapse:collapse;font-size:.88rem;}' +
        '.inv-summary-table td{padding:.3rem .5rem;}' +
        '.inv-summary-table td:last-child{text-align:right;}' +
        '.inv-total-row td{border-top:2px solid #1a1a2e;padding-top:.5rem;font-size:1rem;}' +
        '.inv-footer{display:flex;justify-content:flex-end;}' +
        '.inv-sign{text-align:center;min-width:180px;}' +
        '.inv-sign-img{max-height:60px;max-width:160px;object-fit:contain;margin-bottom:.3rem;}' +
        '.inv-sign-line{border-top:1px solid #333;margin-bottom:.3rem;}' +
        '.inv-bank,.inv-notes,.inv-terms{font-size:.82rem;}' +
        '.inv-bank-text{margin-top:.25rem;}' +
        '.text-end{text-align:right!important;}' +
        '.text-muted{color:#6c757d!important;}' +
        '.fw-semibold{font-weight:600!important;}' +
        '.small{font-size:.875em!important;}' +
        '.mt-2{margin-top:.5rem!important;}' +
        '.mt-3{margin-top:1rem!important;}' +
        '.mt-4{margin-top:1.5rem!important;}' +
        '.ms-2{margin-left:.5rem!important;}' +
        '@page{margin:1.5cm;}';

    var html =
        '<!DOCTYPE html><html><head><meta charset="utf-8"><title>Invoice</title>' +
        '<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css">' +
        '<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css">' +
        '<style>' + invoiceStyles + '</style>' +
        '</head><body>' + el.outerHTML + '</body></html>';

    var printWin = window.open('', '_blank', 'width=950,height=750');
    if (!printWin) {
        alert('Please allow popups to print/download the invoice.');
        return;
    }
    printWin.document.write(html);
    printWin.document.close();
    printWin.focus();
    // Delay so Bootstrap CSS loads in the popup before the print dialog opens
    setTimeout(function () { printWin.print(); printWin.close(); }, 800);
}
