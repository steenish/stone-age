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
const trialOrder = [...trials.keys()];
shuffleArray(trialOrder);
const numTrials = trials.length;
let currentTrialIndex = -1;
const pageDelay = 5000;
const trialDelay = 1000;
let startTime = new Date();
let results = []

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
    $("#referencePage").show();
    setTimeout(() => $("#trialStartButton").prop("disabled", false), pageDelay);
}

function trialStartButtonPressed() {
    $("#referencePage").hide();
    startTrials();
}

function startTrials() {
    $("#trialPage").show();
    setButtonEnableTimer("realismButton", trialDelay);
    nextTrial();
}

function realismSubmit(button) {
    setButtonEnableTimer("realismButton", trialDelay);
    results.push(getTrialResult(button));
    nextTrial();
}

function getTrialResult(button) {
    let imageString = "";
    if (button === "Left") {
        imageString = $("#leftImage").attr("src");
    } else {
        imageString = $("#rightImage").attr("src");
    }
    return {
        trialNum: trialOrder[currentTrialIndex],
        selectedImage: imageString.substring(4, imageString.length - 4),
        selectedButton: button
    }
}

function nextTrial() {
    currentTrialIndex += 1;
    if (currentTrialIndex >= numTrials) {
        finishTrials();
    } else {
        $("#trialNumber").text(`Trial ${currentTrialIndex + 1}`)
        if (Math.random() < 0.5) {
            $("#leftImage").attr("src", `img/${trials[trialOrder[currentTrialIndex]][0]}.png`);
            $("#rightImage").attr("src", `img/${trials[trialOrder[currentTrialIndex]][1]}.png`);
        } else {
            $("#leftImage").attr("src", `img/${trials[trialOrder[currentTrialIndex]][1]}.png`);
            $("#rightImage").attr("src", `img/${trials[trialOrder[currentTrialIndex]][0]}.png`);
        }
    }
}

function finishTrials() {
    $("#trialPage").hide();
    $("#demographicsPage").show();
}

function verifyAndGatherData() {
    let potentialAge  = parseInt($("input[name=age]").val(), 10);
    let potentialCountry = $("input[name=country]").val();
    
    if (Number.isInteger(potentialAge) && potentialCountry) {
        let data = {
            gender: $("select[name=gender]").find(":selected").text(),
            age: potentialAge,
            education: $("select[name=education]").find(":selected").text(),
            country: potentialCountry,
            experience: $("select[name=experience]").find(":selected").text(),
            vision: $("select[name=vision]").find(":selected").text(),
            comments: $("textarea[name=comments]").val(),
            completionCode: generateCompletionCode(10),
            duration: 0.001 * (new Date() - startTime),
            trialResults: results
        }

        var tab = window.open('about:blank', '_blank');
        tab.document.write(`<h1 style="font-family: sans-serif;">Your Mechanical Turk completion code is: ${data.completionCode}</h1>`);
        tab.document.close();

        $("input[name=Data]").val(JSON.stringify(data));
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