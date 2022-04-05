import matplotlib.pyplot as plt
import json

file = open("trialResults.json")
trialJson = file.readlines()[0]
file.close()
data = json.loads(trialJson)

# Data per boxplot.
dataS1 = data["S1"]
dataS2 = data["S2"]
dataS3 = data["S3"]
dataC1 = data["C1"]
dataC2 = data["C2"]
dataC3 = data["C3"]
dataN1 = data["N1"]
dataN2 = data["N2"]
dataN3 = data["N3"]

# --- Combining your data:
data_group1 = [dataS1, dataS2, dataS3]
data_group2 = [dataC1, dataC2, dataC3]
data_group3 = [dataN1, dataN2, dataN3]

# --- Labels for your data:
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

# Offset the positions per group:
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
            widths=width, 
#           notch=False,  
#           vert=True, 
#           whis=1.5,
#           bootstrap=None, 
#           usermedians=None, 
#           conf_intervals=None,
#           patch_artist=False,
            )

plt.boxplot(data_group3, 
            labels=labels_list[6:],
            sym=symbol,
            positions=positions_group3, 
            widths=width, 
#           notch=False,  
#           vert=True, 
#           whis=1.5,
#           bootstrap=None, 
#           usermedians=None, 
#           conf_intervals=None,
#           patch_artist=False,
            )

plt.savefig('boxplot_grouped.png')  
plt.savefig('boxplot_grouped.pdf')    # when publishing, use high quality PDFs
plt.show()                   # uncomment to show the plot. 