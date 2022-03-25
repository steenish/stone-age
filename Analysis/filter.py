import json
from json import tool
from typing import Dict

def main():
    file = open("raw.tsv")
    lines = file.readlines()
    file.close()

    file = open("batchResults.csv")
    batchResults = file.readlines()
    file.close()

    workers = {}

    for i, line in enumerate(batchResults):
        if i > 0:
            values = line.split(",")
            workers[values[-1].strip("\n").strip('"')] = values[-13].strip('"')

    demographics = ["Timestamp\tGender\tAge\tEducation\tCountry\tExperience\tVision\n"]
    lazinessThreshold = 0.5
    tooLazy = []

    for i, line in enumerate(lines):
        if i > 0:
            rawValues = line.split("\t")
            timestamp = rawValues[0]
            data = json.loads(rawValues[1])

            trialData = data["trialResults"]
            counter = 0
            for trial in trialData:
                if trial["selectedButton"] == "Left":
                    counter -= 1
                else:
                    counter += 1
            laziness = counter / len(trialData)

            if abs(laziness) < lazinessThreshold:
                demographics.append(f'{timestamp}\t{data["gender"]}\t{data["age"]}\t{data["education"]}\t{data["country"]}\t{data["experience"]}\t{data["vision"]}\n')
            else:
                tooLazy.append(f'{workers[data["completionCode"]]}, {laziness}')
            
    file = open("demographics.tsv", "w")
    file.writelines(demographics)
    file.close()

    if len(tooLazy) > 0:
        print(f"{len(tooLazy)} worker(s) was/were too lazy:")
        print(tooLazy)
    else:
        print("No workers were too lazy.")

if __name__ == "__main__":
    main()