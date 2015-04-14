function GetFilename(url) 
{
    if (!url) return url;
    var parts = url.split('/');
    if (!parts.length) return url;
    return parts[parts.length - 1];
}

function GetFilePath(url) {
    if (!url) return url;
    var pos = url.lastIndexOf('/');
    if (pos < 0) return url;
    return url.substring(0, pos + 1);
} 

function showHideContent(divCtx, icon) 
{    
    var dv = document.getElementById(divCtx);    
    if (dv.style.display == 'none')
        dv.style.display = 'block';
    else
        dv.style.display = 'none';

    var fname = GetFilename(icon.src);
    var fpath = GetFilePath(icon.src);
    
    if (fname == 'small_arrow_dn.png.png')
        icon.src = fpath + 'picts\small_arrow_up.png';
    else 
        icon.src = fpath + 'picts\small_arrow_dn.png'
}