const trials = [
    ["S1C1C1", "S1C1C2"],
    ["S1C1C1", "S1C1C3"],
    ["S1C1C1", "S1C2C1"],
    ["S1C1C1", "S1C3C1"],
    ["S1C1C1", "S2C1C1"],
    ["S1C1C1", "S3C1C1"],
    ["S1C1C2", "S1C1C3"],
    ["S1C1C2", "S1C2C2"],
    ["S1C1C2", "S1C3C2"],
    ["S1C1C2", "S2C1C2"],
    ["S1C1C2", "S3C1C2"],
    ["S1C1C3", "S1C2C3"],
    ["S1C1C3", "S1C3C3"],
    ["S1C1C3", "S2C1C3"],
    ["S1C1C3", "S3C1C3"],
    ["S1C2C1", "S1C2C2"],
    ["S1C2C1", "S1C2C3"],
    ["S1C2C1", "S1C3C1"],
    ["S1C2C1", "S2C2C1"],
    ["S1C2C1", "S3C2C1"],
    ["S1C2C2", "S1C2C3"],
    ["S1C2C2", "S1C3C2"],
    ["S1C2C2", "S2C2C2"],
    ["S1C2C2", "S3C2C2"],
    ["S1C2C3", "S1C3C3"],
    ["S1C2C3", "S2C2C3"],
    ["S1C2C3", "S3C2C3"],
    ["S1C3C1", "S1C3C2"],
    ["S1C3C1", "S1C3C3"],
    ["S1C3C1", "S2C3C1"],
    ["S1C3C1", "S3C3C1"],
    ["S1C3C2", "S1C3C3"],
    ["S1C3C2", "S2C3C2"],
    ["S1C3C2", "S3C3C2"],
    ["S1C3C3", "S2C3C3"],
    ["S2C1C1", "S2C1C2"],
    ["S2C1C1", "S2C1C3"],
    ["S2C1C1", "S2C2C1"],
    ["S2C1C1", "S2C3C1"],
    ["S2C1C1", "S3C1C1"],
    ["S2C1C2", "S2C1C3"],
    ["S2C1C2", "S2C2C2"],
    ["S2C1C2", "S2C3C2"],
    ["S2C1C2", "S3C1C2"],
    ["S2C1C3", "S2C2C3"],
    ["S2C1C3", "S2C3C3"],
    ["S2C1C3", "S3C1C3"],
    ["S2C2C1", "S2C2C2"],
    ["S2C2C1", "S2C2C3"],
    ["S2C2C1", "S2C3C1"],
    ["S2C2C1", "S3C2C1"],
    ["S2C2C2", "S2C2C3"],
    ["S2C2C2", "S2C3C2"],
    ["S2C2C2", "S3C2C2"],
    ["S2C2C3", "S2C3C3"],
    ["S2C2C3", "S3C2C3"],
    ["S2C3C1", "S2C3C2"],
    ["S2C3C1", "S2C3C3"],
    ["S2C3C1", "S3C3C1"],
    ["S2C3C2", "S2C3C3"],
    ["S2C3C2", "S3C3C2"],
    ["S3C1C1", "S3C1C2"],
    ["S3C1C1", "S3C1C3"],
    ["S3C1C1", "S3C2C1"],
    ["S3C1C1", "S3C3C1"],
    ["S3C1C2", "S3C1C3"],
    ["S3C1C2", "S3C2C2"],
    ["S3C1C2", "S3C3C2"],
    ["S3C1C3", "S3C2C3"],
    ["S3C2C1", "S3C2C2"],
    ["S3C2C1", "S3C2C3"],
    ["S3C2C1", "S3C3C1"],
    ["S3C2C2", "S3C2C3"],
    ["S3C2C2", "S3C3C2"],
    ["S3C3C1", "S3C3C2"]
];
const trialOrder = trials.keys();
shuffleArray(trialOrder);
const numTrials = trials.length;
let currentTrialNumber = -1;
const pageDelay = 500;//0;
const trialDelay = 100;//0;
let startTime = new Date();

setTimeout(() => $("#continueButton").prop("disabled", false), pageDelay);

if (window.screen.height < 600 || window.screen.width < 1000) {
    $("#consentPage").hide();
    $("#screenSizeWarning").show();
    $("#screenSize").text(`Your current screen size is ${window.screen.width}x${window.screen.height}.`);
}

function continueButtonPressed() {
    $("#consentPage").hide();
    $("#instructionPage").show();
    $("#welcomeHeading").hide();
    setTimeout(() => $("#startButton").prop("disabled", false), pageDelay);
}

function startButtonPressed() {
    $("#instructionPage").hide();
    startTrials();
}

function startTrials() {
    $("#trialPage").show();
    setButtonEnableTimer("realismButton", trialDelay);
    
    nextTrial();
}

function realismSubmit(button) { //TODO
    $("#realismQuestion").hide();
    $("#appealQuestion").show();
    
    setButtonEnableTimer("appealButton", trialDelay);
    $(".realismButton").hide();
    $(".appealButton").show();

    realismResults.push(getTrialResult(button));
}

function appealSubmit(button) { //TODO
    $("#realismQuestion").show();
    $("#appealQuestion").hide();
    
    setButtonEnableTimer("realismButton", trialDelay);
    $(".realismButton").show();
    $(".appealButton").hide();

    appealResults.push(getTrialResult(button));

    nextTrial();
}

function getTrialResult(button) { //TODO
    let imageString = "";
    if (button === "Left") {
        imageString = $("#leftImage").attr("src");
    } else {
        imageString = $("#rightImage").attr("src");
    }
    return `${imageString.substring(4, imageString.length - 4)}(${button})`;
}

function nextTrial() { //TODO
    currentTrialNumber += 1;
    if (currentTrialNumber >= numTrials) {
        finishTrials();
    } else {
        $("#trialNumber").text(`Trial ${currentTrialNumber + 1}`)
        if (Math.random() < 0.5) {
            $("#leftImage").attr("src", `img/${trialOrder[currentTrialNumber]}/NL.png`);
            $("#rightImage").attr("src", `img/${trialOrder[currentTrialNumber]}/WL.png`);
        } else {
            $("#leftImage").attr("src", `img/${trialOrder[currentTrialNumber]}/WL.png`);
            $("#rightImage").attr("src", `img/${trialOrder[currentTrialNumber]}/NL.png`);
        }
    }
}

function finishTrials() {
    $("#trialPage").hide();
    $("#demographicsPage").show();
}

function verifyAndGatherData() {
    let data = {
        gender: $("select[name=gender]").find(":selected").text(),
        age: parseInt($("input[name=age]").val(), 10),
        education: $("select[name=education]").find(":selected").text(),
        country: $("input[name=country]").val(),
        experience: $("select[name=experience]").find(":selected").text(),
        vision: $("select[name=vision]").find(":selected").text(),
        comments: $("textarea[name=comments]").val()
    }
    
    if (Number.isInteger(age) && country) {
        data.completionCode = generateCompletionCode(10);
        data.duration = 0.001 * (new Date() - startTime);
        
        // TODO
        // for (var i = 0; i < realismResults.length; ++i) {
        //     $(`input[name=Trial${i + 1}]`).val(`realism[${realismResults[i]}], appeal[${appealResults[i]}]`);
        // }

        alert(`Your Mechanical Turk completion code is: ${data.completionCode}`);

        $("#dataForm").submit();
        $("#demographicsPage").hide();
    } else {
        alert("Incorrect data, correct any errors and try again.");
    }
}

function generateCompletionCode(length) {
    var result = "";
    var characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    var charactersLength = characters.length;
    for (var i = 0; i < length; i++) {
        result += characters.charAt(Math.floor(Math.random() * charactersLength));
    }
    return result;
}

function setButtonEnableTimer(className, delay) {
    $(`.${className}`).prop("disabled", true);
    setTimeout(() => $(`.${className}`).prop("disabled", false), delay);
}

function shuffleArray(array) {
    for (let i = array.length - 1; i > 0; i--) {
      const j = Math.floor(Math.random() * (i + 1));
      const temp = array[i];
      array[i] = array[j];
      array[j] = temp;
    }
}