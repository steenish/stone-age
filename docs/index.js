const trialOrder = ["Rock1Lichen1", "Rock1Lichen2", "Rock1Lichen3", "Rock2Lichen1", "Rock2Lichen2", "Rock2Lichen3", "Rock3Lichen1", "Rock3Lichen2", "Rock3Lichen3", "Tiles1Lichen1", "Tiles1Lichen2", "Tiles1Lichen3", "Tiles2Lichen1", "Tiles2Lichen2", "Tiles2Lichen3", "Tiles3Lichen1", "Tiles3Lichen2", "Tiles3Lichen3"];
shuffleArray(trialOrder);
const numTrials = trialOrder.length;
let currentTrialNumber = -1;
const pageDelay = 5000;
const trialDelay = 1000;
let realismResults = [];
let appealResults = [];
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

function realismSubmit(button) {
    $("#realismQuestion").hide();
    $("#appealQuestion").show();
    
    setButtonEnableTimer("appealButton", trialDelay);
    $(".realismButton").hide();
    $(".appealButton").show();

    realismResults.push(getTrialResult(button));
}

function appealSubmit(button) {
    $("#realismQuestion").show();
    $("#appealQuestion").hide();
    
    setButtonEnableTimer("realismButton", trialDelay);
    $(".realismButton").show();
    $(".appealButton").hide();

    appealResults.push(getTrialResult(button));

    nextTrial();
}

function getTrialResult(button) {
    let imageString = "";
    if (button === "Left") {
        imageString = $("#leftImage").attr("src");
    } else {
        imageString = $("#rightImage").attr("src");
    }
    return `${imageString.substring(4, imageString.length - 4)}(${button})`;
}

function nextTrial() {
    currentTrialNumber += 1;
    if (currentTrialNumber >= numTrials) {
        finishTrials();
    } else {
        $("#trialNumber").text(`Trial ${currentTrialNumber + 1}`)
        $("#referenceImage").attr("src", `img/${trialOrder[currentTrialNumber]}/REF.png`); // <-- will be png later
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
    let gender = $("select[name=gender]").find(":selected").text();
    let age = parseInt($("input[name=age]").val(), 10);
    let education = $("select[name=education]").find(":selected").text();
    let country = $("input[name=country]").val();
    let experience = $("select[name=experience]").find(":selected").text();
    let comments = $("textarea[name=comments]").val();
    
    if (Number.isInteger(age) && country) {
        let completionCode = generateCompletionCode(10);
        let duration = 0.001 * (new Date() - startTime);
        
        $("input[name=Gender]").val(gender);
        $("input[name=Age]").val(age);
        $("input[name=Education]").val(education);
        $("input[name=Country]").val(country);
        $("input[name=Experience]").val(experience);
        $("input[name=Comments]").val(comments);
        $("input[name=Duration]").val(duration);
        $("input[name=CompletionCode]").val(completionCode);
        
        for (var i = 0; i < realismResults.length; ++i) {
            $(`input[name=Trial${i + 1}]`).val(`realism[${realismResults[i]}], appeal[${appealResults[i]}]`);
        }

        alert(`Your Mechanical Turk completion code is: ${completionCode}`);

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