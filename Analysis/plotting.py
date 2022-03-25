import matplotlib.pyplot as plt
import seaborn as sns
import pandas as pd
import numpy as np

sns.set(style="whitegrid")

lichen = pd.read_csv("results.csv")
ax = sns.boxplot(x="measure", y="proportion", hue="lichen", data=lichen, linewidth=1.5, width=0.5)

plt.title("Realism and appeal scores")
plt.ylabel("Proportion of trials selected as most realistic")
plt.yticks(np.arange(0, 110, 10))
plt.xlabel("")
ax.set_xticklabels(["Realism", "Appeal"])
plt.grid(visible=True, which="major", axis="y")
plt.legend(title="Lichen")
plt.show()