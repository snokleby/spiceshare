function setSpPrefetch(p) {
    var h = document.createElement("link")
    h.setAttribute("rel", "prefetch")
    h.setAttribute("href", p)
    document.getElementsByTagName("head")[0].appendChild(h);
}