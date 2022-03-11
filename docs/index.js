const trialOrder = ["Rock1Lichen1", "Rock1Lichen2", "Rock1Lichen3", "Rock2Lichen1", "Rock2Lichen2", "Rock2Lichen3", "Rock3Lichen1", "Rock3Lichen2", "Rock3Lichen3", "Tiles1Lichen1", "Tiles1Lichen2", "Tiles1Lichen3", "Tiles2Lichen1", "Tiles2Lichen2", "Tiles2Lichen3", "Tiles3Lichen1", "Tiles3Lichen2", "Tiles3Lichen3"];
shuffleArray(trialOrder)
const numTrials = trialOrder.length;
let currentTrialNumber = -1;
const pageDelay = 100;
const trialDelay = 500;

setTimeout(() => $("#continueButton").prop("disabled", false), pageDelay);

function continueButtonPressed() {
    $("#instructionPage").hide();
    $("#consentPage").show();
    $("#welcomeHeading").css("visibility", "hidden");
    setTimeout(() => $("#startButton").prop("disabled", false), pageDelay);
}

function startButtonPressed() {
    $("#consentPage").hide();
    startTrials();
}

function startTrials() {
    $("#trialPage").show();
    setButtonEnableTimer("realismButton", trialDelay);
    
    nextTrial();
}

function realismSubmit() {
    $("#realismQuestion").hide();
    $("#appealQuestion").show();
    
    setButtonEnableTimer("appealButton", trialDelay);
    $(".realismButton").hide();
    $(".appealButton").show();
    
    // TODO
}

function appealSubmit() {
    $("#realismQuestion").show();
    $("#appealQuestion").hide();
    
    setButtonEnableTimer("realismButton", trialDelay);
    $(".realismButton").show();
    $(".appealButton").hide();

    // TODO

    nextTrial();
}

function nextTrial() {
    currentTrialNumber += 1;
    if (currentTrialNumber >= numTrials) {
        finishTrials();
    }

    $("#trialNumber").text(`Trial ${currentTrialNumber + 1}`)
    $("#referenceImage").attr("src", `img/${trialOrder[currentTrialNumber]}/REF.jpg`); // <-- will be png later
    if (Math.random() < 0.5) {
        $("#leftImage").attr("src", `img/${trialOrder[currentTrialNumber]}/NL.png`);
        $("#rightImage").attr("src", `img/${trialOrder[currentTrialNumber]}/WL.png`);
    } else {
        $("#leftImage").attr("src", `img/${trialOrder[currentTrialNumber]}/WL.png`);
        $("#rightImage").attr("src", `img/${trialOrder[currentTrialNumber]}/NL.png`);
    }
    
    // TODO
}

function finishTrials() {
    $("#trialPage").hide();

    // TODO
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