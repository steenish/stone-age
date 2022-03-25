import re
from collections import Counter

def main():
    file = open("raw.tsv")
    lines = file.readlines()
    file.close()

    realismCount = [
        ("Rock1Lichen1", 0),
        ("Rock1Lichen2", 0),
        ("Rock1Lichen3", 0),
        ("Rock2Lichen1", 0),
        ("Rock2Lichen2", 0),
        ("Rock2Lichen3", 0),
        ("Rock3Lichen1", 0),
        ("Rock3Lichen2", 0),
        ("Rock3Lichen3", 0),
        ("Tiles1Lichen1", 0),
        ("Tiles1Lichen2", 0),
        ("Tiles1Lichen3", 0),
        ("Tiles2Lichen1", 0),
        ("Tiles2Lichen2", 0),
        ("Tiles2Lichen3", 0),
        ("Tiles3Lichen1", 0),
        ("Tiles3Lichen2", 0),
        ("Tiles3Lichen3", 0)
    ]
    appealCount = [
        ("Rock1Lichen1", 0),
        ("Rock1Lichen2", 0),
        ("Rock1Lichen3", 0),
        ("Rock2Lichen1", 0),
        ("Rock2Lichen2", 0),
        ("Rock2Lichen3", 0),
        ("Rock3Lichen1", 0),
        ("Rock3Lichen2", 0),
        ("Rock3Lichen3", 0),
        ("Tiles1Lichen1", 0),
        ("Tiles1Lichen2", 0),
        ("Tiles1Lichen3", 0),
        ("Tiles2Lichen1", 0),
        ("Tiles2Lichen2", 0),
        ("Tiles2Lichen3", 0),
        ("Tiles3Lichen1", 0),
        ("Tiles3Lichen2", 0),
        ("Tiles3Lichen3", 0)
    ]
    for i, line in enumerate(lines):
        if i > 0:
            rawValues = line.split("\t")[10:]
            for value in rawValues:
                match = re.search(r"realism\[(.+)/([NW]L)\(.+\)\], appeal\[(.+)/([NW]L)\(.+\)\]", value)
                
                realismName = match.group(1)
                realismLichenPreferred = match.group(2) == "WL"
                for j, count in enumerate(realismCount):
                    if count[0] == realismName and realismLichenPreferred:
                        realismCount[j] = (realismName, count[1] + 1)

                appealName = match.group(3)
                appealLichenPreferred = match.group(4) == "WL"
                for j, count in enumerate(appealCount):
                    if count[0] == appealName and appealLichenPreferred:
                        appealCount[j] = (appealName, count[1] + 1)
    
    result = ["name,proportion,lichen,measure\n"]
    for count in realismCount:
        proportion = 100 * count[1] / (len(lines) - 1)
        result.append(f"{count[0]},{int(proportion)},WL,realism\n")
        result.append(f"{count[0]},{int(100 - proportion)},NL,realism\n")

    for count in appealCount:
        proportion = int(100 * count[1] / (len(lines) - 1))
        result.append(f"{count[0]},{proportion},WL,appeal\n")
        result.append(f"{count[0]},{100 - proportion},NL,appeal\n")

    file = open("results.csv", "w")
    file.writelines(result)
    file.close()

if __name__ == "__main__":
    main()