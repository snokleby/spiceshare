 
(function() {
    var mod = 'modernizr';
    try {
        localStorage.setItem(mod, mod);
        localStorage.removeItem(mod);
    } catch (e) {
        document.getElementById("storeurl").className = "";
    }
})();

spiceforms.SetPostUrl("/User/_PrivateUserPageAjax");

document.getElementById('updateuserloc').addEventListener('click', function (e) {
    spiceGeo.GetGeoLocIntoFields(function () {
        spiceforms.PostUserFormAjax();
    });
    e.preventDefault();
});
