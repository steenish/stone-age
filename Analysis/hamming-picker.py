def hammingDistance(string1, string2):
    if len(string1) != len(string2):
        return 0
    distance = 0
    for letter1, letter2 in zip(string1, string2):
        if letter1 != letter2:
            distance += 1
    return distance

def main():
    file = open("settingNames.csv")
    settingNames = [x[:-1] for x in file.readlines()]
    file.close()
    results = []

    for i, setting in enumerate(settingNames):
        for secondSetting in settingNames[i:]:
            if (hammingDistance(setting, secondSetting) == 1):
                results.append(f"{setting},{secondSetting}\n")
    
    file = open("pairs.csv", "w")
    file.writelines(results)
    file.close()

if __name__ == "__main__":
    main()