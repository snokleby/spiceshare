(function() {
    try {
        var userPageKey = "userpage";
        var userIdenitity = localStorage.getItem(userPageKey);
        if (userIdenitity !== null) {
            var uid = JSON.parse(userIdenitity);
            var area = document.getElementById("privlinkar");
            area.appendChild(document.createTextNode("Great, It looks like you're already a user."));
            var link = document.createElement("a");
            link.href = "/User/PrivateUserPage?identity=" + uid.id;
            link.className = "act";
            link.innerText = "Go to your profile";
            area.appendChild(link);
            area.style.display = "";
           
        }
    } catch (ex) {
    
    }

})();