import json

def main():
    # Read the raw results straight from the experiments.
    file = open("raw.tsv")
    lines = file.readlines()
    file.close()

    # Read the results exported from MTurk.
    file = open("batchResults.csv")
    batchResults = file.readlines()
    file.close()

    # Initialize parameters.
    lazinessThreshold = 0.5

    # Initialize demographics results.
    demographics = ["Timestamp\tGender\tAge\tEducation\tCountry\tExperience\tVision\tDuration\n"]
    
    # Initialize list of lazy participants.
    tooLazy = []

    # List of all the trial configurations, with the same order as the experiment data will hold.
    trials = [
        ("S1C1C1", "S1C1C2"),
        ("S1C1C1", "S1C1C3"),
        ("S1C1C1", "S1C2C1"),
        ("S1C1C1", "S1C3C1"),
        ("S1C1C1", "S2C1C1"),
        ("S1C1C1", "S3C1C1"),
        ("S1C1C2", "S1C1C3"),
        ("S1C1C2", "S1C2C2"),
        ("S1C1C2", "S1C3C2"),
        ("S1C1C2", "S2C1C2"),
        ("S1C1C2", "S3C1C2"),
        ("S1C1C3", "S1C2C3"),
        ("S1C1C3", "S1C3C3"),
        ("S1C1C3", "S2C1C3"),
        ("S1C1C3", "S3C1C3"),
        ("S1C2C1", "S1C2C2"),
        ("S1C2C1", "S1C2C3"),
        ("S1C2C1", "S1C3C1"),
        ("S1C2C1", "S2C2C1"),
        ("S1C2C1", "S3C2C1"),
        ("S1C2C2", "S1C2C3"),
        ("S1C2C2", "S1C3C2"),
        ("S1C2C2", "S2C2C2"),
        ("S1C2C2", "S3C2C2"),
        ("S1C2C3", "S1C3C3"),
        ("S1C2C3", "S2C2C3"),
        ("S1C2C3", "S3C2C3"),
        ("S1C3C1", "S1C3C2"),
        ("S1C3C1", "S1C3C3"),
        ("S1C3C1", "S2C3C1"),
        ("S1C3C1", "S3C3C1"),
        ("S1C3C2", "S1C3C3"),
        ("S1C3C2", "S2C3C2"),
        ("S1C3C2", "S3C3C2"),
        ("S1C3C3", "S2C3C3"),
        ("S2C1C1", "S2C1C2"),
        ("S2C1C1", "S2C1C3"),
        ("S2C1C1", "S2C2C1"),
        ("S2C1C1", "S2C3C1"),
        ("S2C1C1", "S3C1C1"),
        ("S2C1C2", "S2C1C3"),
        ("S2C1C2", "S2C2C2"),
        ("S2C1C2", "S2C3C2"),
        ("S2C1C2", "S3C1C2"),
        ("S2C1C3", "S2C2C3"),
        ("S2C1C3", "S2C3C3"),
        ("S2C1C3", "S3C1C3"),
        ("S2C2C1", "S2C2C2"),
        ("S2C2C1", "S2C2C3"),
        ("S2C2C1", "S2C3C1"),
        ("S2C2C1", "S3C2C1"),
        ("S2C2C2", "S2C2C3"),
        ("S2C2C2", "S2C3C2"),
        ("S2C2C2", "S3C2C2"),
        ("S2C2C3", "S2C3C3"),
        ("S2C2C3", "S3C2C3"),
        ("S2C3C1", "S2C3C2"),
        ("S2C3C1", "S2C3C3"),
        ("S2C3C1", "S3C3C1"),
        ("S2C3C2", "S2C3C3"),
        ("S2C3C2", "S3C3C2"),
        ("S3C1C1", "S3C1C2"),
        ("S3C1C1", "S3C1C3"),
        ("S3C1C1", "S3C2C1"),
        ("S3C1C1", "S3C3C1"),
        ("S3C1C2", "S3C1C3"),
        ("S3C1C2", "S3C2C2"),
        ("S3C1C2", "S3C3C2"),
        ("S3C1C3", "S3C2C3"),
        ("S3C2C1", "S3C2C2"),
        ("S3C2C1", "S3C2C3"),
        ("S3C2C1", "S3C3C1"),
        ("S3C2C2", "S3C2C3"),
        ("S3C2C2", "S3C3C2"),
        ("S3C3C1", "S3C3C2")
    ]
    # Initialize scoring results.
    # Order of elements:
    # - Very first index is just the index of the participant
    # - First sub-index is parameter group (S, C, N)
    # - (Second, Third) sub-indices are e.g. with S1 vs S2, S1's score is in (0, 1) and S2's score is in (1, 0)
    participantScores = []

    # For each participant.
    for i, line in enumerate(lines):
        # Skip first line, which is a header.
        if i > 0:
            rawValues = line.split("\t")
            timestamp = rawValues[0]
            data = json.loads(rawValues[1])
            trialData = data["trialResults"]

            # Figure out if worker is lazy.
            counter = 0
            for trial in trialData:
                if trial["selectedButton"] == "Left":
                    counter -= 1
                else:
                    counter += 1
            laziness = counter / len(trialData)

            # If worker is not lazy, store demographics and calculate scores.
            if abs(laziness) < lazinessThreshold:
                demographics.append(f'{timestamp}\t{data["gender"]}\t{data["age"]}\t{data["education"]}\t{data["country"]}\t{data["experience"]}\t{data["vision"]}\t{data["duration"]}\n')

                participantScore = [
                        [
                            [0, 0, 0],
                            [0, 0, 0],
                            [0, 0, 0]
                        ],
                                    [
                                        [0, 0, 0],
                                        [0, 0, 0],
                                        [0, 0, 0]
                                    ],
                                                [
                                                    [0, 0, 0],
                                                    [0, 0, 0],
                                                    [0, 0, 0]
                                                ]
                    ]
                for trial in trialData:
                    trialIndex = trial["trialNum"]
                    selectedImage = trial["selectedImage"]
                    otherImage = trials[trialIndex][0]
                    if selectedImage == otherImage:
                        otherImage = trials[trialIndex][1]
                    
                    # Map (index, value) where image names differ -> index of score to increment.
                    diffIndex = 0
                    for j, c in enumerate(selectedImage):
                        if c != otherImage[j]:
                            diffIndex = j
                            break
                    parameterGroupIndex = int(0.5 * (diffIndex - 1))
                    selectedIndex = int(selectedImage[diffIndex]) - 1
                    otherIndex = int(otherImage[diffIndex]) - 1

                    # Modify the scores.
                    participantScore[parameterGroupIndex][selectedIndex][otherIndex] += 1
                    participantScore[parameterGroupIndex][otherIndex][selectedIndex] -= 1
                
                participantScores.append(participantScore)
            else:
                tooLazy.append(f'{data["completionCode"]}, {laziness}')
    
    trialResults = ["S1S2_1\tS1S2_2\tS1S3_1\tS1S3_3\tS2S3_2\tS2S3_3\tC1C2_1\tC1C2_2\tC1C3_1\tC1C3_3\tC2C3_2\tC2C3_3\tN1N2_1\tN1N2_2\tN1N3_1\tN1N3_3\tN2N3_2\tN2N3_3\n"]
    trialResultsDict = { "S1S2_1": [], "S1S2_2": [], "S1S3_1": [], "S1S3_3": [], "S2S3_2": [], "S2S3_3": [], "C1C2_1": [], "C1C2_2": [], "C1C3_1": [], "C1C3_3": [], "C2C3_2": [], "C2C3_3": [], "N1N2_1": [], "N1N2_2": [], "N1N3_1": [], "N1N3_3": [], "N2N3_2": [], "N2N3_3": [] }
    for score in participantScores:
        trialResults.append(f"{score[0][0][1]}\t{score[0][1][0]}\t{score[0][0][2]}\t{score[0][2][0]}\t{score[0][1][2]}\t{score[0][2][1]}\t{score[1][0][1]}\t{score[1][1][0]}\t{score[1][0][2]}\t{score[1][2][0]}\t{score[1][1][2]}\t{score[1][2][1]}\t{score[2][0][1]}\t{score[2][1][0]}\t{score[2][0][2]}\t{score[2][2][0]}\t{score[2][1][2]}\t{score[2][2][1]}\n")
        trialResultsDict["S1S2_1"].append(score[0][0][1])
        trialResultsDict["S1S2_2"].append(score[0][1][0])
        trialResultsDict["S1S3_1"].append(score[0][0][2])
        trialResultsDict["S1S3_3"].append(score[0][2][0])
        trialResultsDict["S2S3_2"].append(score[0][1][2])
        trialResultsDict["S2S3_3"].append(score[0][2][1])

        trialResultsDict["C1C2_1"].append(score[1][0][1])
        trialResultsDict["C1C2_2"].append(score[1][1][0])
        trialResultsDict["C1C3_1"].append(score[1][0][2])
        trialResultsDict["C1C3_3"].append(score[1][2][0])
        trialResultsDict["C2C3_2"].append(score[1][1][2])
        trialResultsDict["C2C3_3"].append(score[1][2][1])

        trialResultsDict["N1N2_1"].append(score[2][0][1])
        trialResultsDict["N1N2_2"].append(score[2][1][0])
        trialResultsDict["N1N3_1"].append(score[2][0][2])
        trialResultsDict["N1N3_3"].append(score[2][2][0])
        trialResultsDict["N2N3_2"].append(score[2][1][2])
        trialResultsDict["N2N3_3"].append(score[2][2][1])

    file = open("demographics.tsv", "w")
    file.writelines(demographics)
    file.close()

    file = open("trialResults.tsv", "w")
    file.writelines(trialResults)
    file.close()

    with open("trialResults.json", 'w') as fp:
        json.dump(trialResultsDict, fp)

    if len(tooLazy) > 0:
        print(f"{len(tooLazy)} worker(s) was/were too lazy:")
        print(tooLazy)
    else:
        print("No workers were too lazy.")

if __name__ == "__main__":
    main()