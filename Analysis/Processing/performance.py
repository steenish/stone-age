import matplotlib.pyplot as plt

data = {
    "S1": 2128.841111,
    "S2": 237.5068148,
    "S3": 23.4182963,
    "C1": 1300.619074,
    "C2": 443.9235926,
    "C3": 645.2235556,
    "N1": 172.8032963,
    "N2": 767.0983704,
    "N3": 1449.864556
}
labels = list(data.keys())
values = list(data.values())

fig = plt.figure(figsize = (5, 5))

barlist = plt.bar(labels, values, width = 0.4)

barlist[0].set_color("#0B84A5")
barlist[1].set_color("#0B84A5")
barlist[2].set_color("#0B84A5")

barlist[3].set_color("#F6C85F")
barlist[4].set_color("#F6C85F")
barlist[5].set_color("#F6C85F")

barlist[6].set_color("#6F4E7C")
barlist[7].set_color("#6F4E7C")
barlist[8].set_color("#6F4E7C")

plt.grid(True, linestyle="dotted", axis="y")

ax = plt.gca()
ax.set_yticks(range(0, 2201, 100))
ax.set_ylim(0, 2200)
plt.xlabel("Parameter setting")
plt.ylabel("Execution time (s)")
plt.title("Execution time by parameter setting")

plt.gcf().subplots_adjust(left=0.15)

plt.savefig("Plots/performance.png")
plt.savefig("Plots/performance.pdf")
plt.show()