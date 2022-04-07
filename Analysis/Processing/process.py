import json
from json import tool
from typing import Dict
import matplotlib.pyplot as plt

def main():
    ######################## FILTERING START ########################

    # Get filename of data file.
    filename = input("Enter raw data filename (without .tsv): ")

    # Read the raw results straight from the experiments.
    lines = []
    try:
        file = open(f"Raw/{filename}.tsv")
        lines = file.readlines()
        file.close()
    except:
        print("Error reading file.")
        return

    # Initialize parameters.
    lazinessThreshold = 0.5
    numParticipants = len(lines)

    # Initialize demographics and comments results.
    demographics = ["Timestamp\tGender\tAge\tEducation\tCountry\tExperience\tVision\tDuration\tComments\n"]
    
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
                comments = data["comments"].replace("\n", " ")
                demographics.append(f'{timestamp}\t{data["gender"]}\t{data["age"]}\t{data["education"]}\t{data["country"]}\t{data["experience"]}\t{data["vision"]}\t{data["duration"]}\t{comments}\n')

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
                tooLazy.append(f'participant {i}, {laziness}')
    
    trialResults = ["S1\tS2\tS3\tC1\tC2\tC3\tc1\tc2\tc3\n"]
    trialResultsDict = { "S1": [], "S2": [], "S3": [], "C1": [], "C2": [], "C3": [], "N1": [], "N2": [], "N3": [] }
    for score in participantScores:
        trialResults.append(f"{score[0]}\t{score[1]}\t{score[2]}\t{score[3]}\t{score[4]}\t{score[5]}\t{score[6]}\t{score[7]}\t{score[8]}\n")
        trialResultsDict["S1"].append(score[0])
        trialResultsDict["S2"].append(score[1])
        trialResultsDict["S3"].append(score[2])
        trialResultsDict["C1"].append(score[3])
        trialResultsDict["C2"].append(score[4])
        trialResultsDict["C3"].append(score[5])
        trialResultsDict["N1"].append(score[6])
        trialResultsDict["N2"].append(score[7])
        trialResultsDict["N3"].append(score[8])

    file = open(f"Processed/{filename}_demographics.tsv", "w")
    file.writelines(demographics)
    file.close()

    file = open(f"Processed/{filename}_trialResults.tsv", "w")
    file.writelines(trialResults)
    file.close()

    with open(f"Processed/{filename}_trialResults.json", "w") as fp:
        json.dump(trialResultsDict, fp)

    if len(tooLazy) > 0:
        print(f"{len(tooLazy)} worker(s) was/were too lazy:")
        print(tooLazy)
    else:
        print("No workers were too lazy.")



    ######################## FILTERING END ########################

    ######################## PLOTTING START ########################



    # Data per boxplot.
    dataS1 = trialResultsDict["S1"]
    dataS2 = trialResultsDict["S2"]
    dataS3 = trialResultsDict["S3"]
    dataC1 = trialResultsDict["C1"]
    dataC2 = trialResultsDict["C2"]
    dataC3 = trialResultsDict["C3"]
    dataN1 = trialResultsDict["N1"]
    dataN2 = trialResultsDict["N2"]
    dataN3 = trialResultsDict["N3"]

    data_group1 = [dataS1, dataS2, dataS3]
    data_group2 = [dataC1, dataC2, dataC3]
    data_group3 = [dataN1, dataN2, dataN3]

    labels_list = ["S1", "S2", "S3", "C1", "C2", "C3", "N1", "N2", "N3"]
    xlocations  = range(len(labels_list))
    width       = 0.2
    symbol      = 'r+'
    ymin        = -20
    ymax        = 20

    ax = plt.gca()
    ax.set_ylim(ymin,ymax)
    ax.grid(True, linestyle='dotted')
    ax.set_axisbelow(True)
    plt.xlabel("Parameter group")
    plt.ylabel("Score")
    plt.title("Score by parameter group")

    # Offset the positions per group.
    positions_group1 = [xlocations[0] - (width + 0.01), xlocations[0], xlocations[0] + (width + 0.01)]
    positions_group2 = [xlocations[1] - (width + 0.01), xlocations[1], xlocations[1] + (width + 0.01)]
    positions_group3 = [xlocations[2] - (width + 0.01), xlocations[2], xlocations[2] + (width + 0.01)]

    plt.boxplot(data_group1, 
                sym=symbol,
                labels=labels_list[:3],
                positions=positions_group1, 
                widths=width, 
    #           notch=False,  
    #           vert=True, 
    #           whis=1.5,
    #           bootstrap=None, 
    #           usermedians=None, 
    #           conf_intervals=None,
    #           patch_artist=False,
                )

    plt.boxplot(data_group2, 
                labels=labels_list[3:6],
                sym=symbol,
                positions=positions_group2, 
                widths=width
                )

    plt.boxplot(data_group3, 
                labels=labels_list[6:],
                sym=symbol,
                positions=positions_group3, 
                widths=width
                )

    plt.savefig(f"Plots/{filename}_boxplot.png")  
    plt.savefig(f"Plots/{filename}_boxplot.pdf")
    #plt.show()                   # uncomment to show the plot.

    ######################## PLOTTING END ########################
    

if __name__ == "__main__":
    main()