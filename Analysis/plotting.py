import matplotlib.pyplot as pl
import json

file = open("trialResults.json")
trialJson = file.readlines()[0]
file.close()
data = json.loads(trialJson)

# Data per box plot.
dataS1S2_1 = data["S1S2_1"]
dataS1S2_2 = data["S1S2_2"]
dataS1S3_1 = data["S1S3_1"]
dataS1S3_3 = data["S1S3_3"]
dataS2S3_2 = data["S2S3_2"]
dataS2S3_3 = data["S2S3_3"]
dataC1C2_1 = data["C1C2_1"]
dataC1C2_2 = data["C1C2_2"]
dataC1C3_1 = data["C1C3_1"]
dataC1C3_3 = data["C1C3_3"]
dataC2C3_2 = data["C2C3_2"]
dataC2C3_3 = data["C2C3_3"]
dataN1N2_1 = data["N1N2_1"]
dataN1N2_2 = data["N1N2_2"]
dataN1N3_1 = data["N1N3_1"]
dataN1N3_3 = data["N1N3_3"]
dataN2N3_2 = data["N2N3_2"]
dataN2N3_3 = data["N2N3_3"]

# Combining data:
data_group1 = [dataS1S2_1, dataS1S2_2]
data_group2 = [dataS1S3_1, dataS1S3_3]
data_group3 = [dataS2S3_2, dataS2S3_3]
data_group4 = [dataC1C2_1, dataC1C2_2]
data_group5 = [dataC1C3_1, dataC1C3_3]
data_group6 = [dataC2C3_2, dataC2C3_3]
data_group7 = [dataN1N2_1, dataN1N2_2]
data_group8 = [dataN1N3_1, dataN1N3_3]
data_group9 = [dataN2N3_2, dataN2N3_3]

data_groups = [data_group1, data_group2, data_group3, data_group4, data_group5, data_group6, data_group7, data_group8, data_group9]

# Labels for data:
labels_list = ["S1, S2", "S1, S3", "S2, S3", "C1, C2", "C1, C3", "C2, C3", "N1, N2", "N1, N3", "N2, N3"]
width       = 0.3
xlocations  = [ x * ((1 + len(data_groups)) * width) for x in range(len(data_group1)) ]

symbol      = 'r+'
# ymin        = min ( [ val  for dg in data_groups  for data in dg for val in data ] )
# ymax        = max ( [ val  for dg in data_groups  for data in dg for val in data ] )
ymin        = -10
ymax        = 10

ax = pl.gca()
ax.set_ylim(ymin,ymax)

ax.grid(True, linestyle='dotted')
ax.set_axisbelow(True)

pl.xlabel('X axis label')
pl.ylabel('Y axis label')
pl.title('title')

space = len(data_groups)/2
offset = len(data_groups)/2

ax.set_xticks( xlocations )
ax.set_xticklabels( labels_list, rotation=0 )
# --- Offset the positions per group:

group_positions = []
for num, dg in enumerate(data_groups):    
    _off = (0 - space + (0.5+num))
    print(_off)
    group_positions.append([x-_off*(width+0.01) for x in xlocations])

for dg, pos in zip(data_groups, group_positions):
    pl.boxplot(dg, 
                sym=symbol,
    #            labels=['']*len(labels_list),
                labels=['']*len(labels_list),           
                positions=pos, 
                widths=width, 
    #           notch=False,  
    #           vert=True, 
    #           whis=1.5,
    #           bootstrap=None, 
    #           usermedians=None, 
    #           conf_intervals=None,
    #           patch_artist=False,
                )



pl.show()