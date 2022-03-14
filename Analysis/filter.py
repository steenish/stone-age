import re

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
                
    print(realismCount)
    print(appealCount)


if __name__ == "__main__":
    main()