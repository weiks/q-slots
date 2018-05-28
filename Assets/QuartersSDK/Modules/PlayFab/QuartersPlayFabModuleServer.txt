var TRANSFER_URL = "https://api.pocketfulofquarters.com";
var SERVER_TOKEN = "l1jsez9od0r906tsz3rwqhd0prwgxh2gb9o9iarovutic3wt74g0mmt"; //ENTER SERVER TOKEN HERE
var APP_ADDRESS = "0x365bd2c36a92f4228d7f0d6d6265e8637671be0d"; //ENTER APP ADDRESS HERE


//PlayFab Cloud script handler
handlers.AwardQuarters = function (args, context) {
    var result = AwardQuarters(args);
    return result;
};



function AwardQuarters (args) {

    var amount = args["amount"];
    var user = args["userId"];

    if (SERVER_TOKEN == "") {
        log.error("Missing SERVER_TOKEN parameter");
        return;
    }
    if (APP_ADDRESS == "") {
        log.error("Missing APP_ADDRESS parameter");
        return;
    }
    if (amount == undefined || amount == null) {
        log.error("Missing amount parameter");
        return;
    }
    if (user == undefined || user == null) {
        log.error("Missing user parameter");
        return;
    }

    log.info("Award " + amount + " quarters to user: " + user);


    var requestAuthorized = true;

    /*
       ######## ENTER YOUR CUSTOM LOGIC HERE,
       BY DEFAULT EVERY AWARD REQUEST IS ACCEPTED


       var shouldReceiveDailyBonus = !DidReceivedBonusToday(userId); //DidReceivedBonusToday could be a custom logic method checking Playfab user data

       if (shouldReceiveDailyBonus == false) {
           log.error("Daily bonus already given today");
           requestAuthorized = false;
       }

       if (amount != DAILY_BONUS_AMOUNT) {
           log.error("Requested daily bonus amount is incorrect");
           requestAuthorized = false;
       }


    */


    if (!requestAuthorized) return { error: "Award Quarters request not authorized by game server" };

    var postData = {
        "amount": amount,
        "user": user
    };

    var url = TRANSFER_URL + "/v1/accounts/" + APP_ADDRESS + "/transfer";

    var headers = {
        'Authorization': 'Bearer ' + SERVER_TOKEN,
        'Content-Type': 'application/json;charset=UTF-8'
    };

    var contentType = "application/json";
    var contentBody = JSON.stringify(postData);
    var response = http.request(url, "post", contentBody, contentType, headers);

    return response;
}