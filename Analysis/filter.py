import json
from json import tool
from typing import Dict

def main():
    # Read the raw results straight from the experiments.
    file = open("raw.tsv")
    lines = file.readlines()
    file.close()

    # Read the results exported from MTurk.
    file = open("batchResults.csv")
    batchResults = file.readlines()
    file.close()

    # Create dictionary of completionCode -> workerId.
    workers = {}
    for i, line in enumerate(batchResults):
        if i > 0:
            values = line.split(",")
            workers[values[-1].strip("\n").strip('"')] = values[-13].strip('"')

    # Initialize parameters.
    lazinessThreshold = 0.5
    numParticipants = len(lines)

    # Initialize demographics results.
    demographics = ["Timestamp\tGender\tAge\tEducation\tCountry\tExperience\tVision\n"]
    
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
    # Order of elements: S1, S2, S3, C1, C2, C3, c1, c2, c3 (where c is number of clusters)
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
                demographics.append(f'{timestamp}\t{data["gender"]}\t{data["age"]}\t{data["education"]}\t{data["country"]}\t{data["experience"]}\t{data["vision"]}\n')

                # This participant's scores, S1, S2, S3, C1, C2, C3, c1, c2, c3.
                participantScore = [0, 0, 0, 0, 0, 0, 0, 0, 0]
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
                    scoreIndex = int(1.5 * (diffIndex - 1))
                    minusIndex = scoreIndex + int(otherImage[diffIndex]) - 1
                    scoreIndex += int(selectedImage[diffIndex]) - 1

                    # Increment the score.
                    participantScore[scoreIndex] += 1
                    participantScore[minusIndex] -= 1
                
                participantScores.append(participantScore)
            else:
                tooLazy.append(f'{workers[data["completionCode"]]}, {laziness}')
    
    trialResults = ["S1\tS2\tS3\tC1\tC2\tC3\tc1\tc2\tc3\n"]
    for score in participantScores:
        trialResults.append(f"{score[0]}\t{score[1]}\t{score[2]}\t{score[3]}\t{score[4]}\t{score[5]}\t{score[6]}\t{score[7]}\t{score[8]}\n")

    file = open("demographics.tsv", "w")
    file.writelines(demographics)
    file.close()

    file = open("trialResults.tsv", "w")
    file.writelines(trialResults)
    file.close()

    if len(tooLazy) > 0:
        print(f"{len(tooLazy)} worker(s) was/were too lazy:")
        print(tooLazy)
    else:
        print("No workers were too lazy.")

if __name__ == "__main__":
    main()