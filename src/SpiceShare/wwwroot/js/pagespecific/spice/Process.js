(function () {
    var elm = document.getElementById("procid");
    var spiceId = elm.dataset.spiceidentity;
    elm.innerText = "Processing your image. Please wait."
    setInterval(function () {
        elm.innerText += ".";
    }, 750);
    window.location.replace(document.location.origin + "/Spice/_ProcessImage?SpiceIdentity=" + spiceId);

})();