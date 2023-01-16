import json

filePath= "../Data/old_sus/run3-pop_4096_health_100_energy_50.json"#"run2-pop_4096_health_100_energy_50_foodCount_best_init_progressive.json"


#Import and prepare data

data = {}

with open(filePath,"r") as file:
    data = json.load(file)


epochs = []

#Fitness
fitness_average = []
fitness_max = []


#Sizes
sizes_average = []
sizes_top = []
sizes_worst = []

#Speeds
speeds_average = []
speeds_top = []
speeds_worst = []

for epoch in data:
    epochs.append(epoch)
    for metric in data[epoch]:
        if metric == "fitness":
            fitness_average.append(data[epoch][metric]["average"])
            fitness_max.append(data[epoch][metric]["max"])
        elif metric == "size":
            sizes_average.append(data[epoch][metric]["average"])
            sizes_top.append(data[epoch][metric]["top"])
            sizes_worst.append(data[epoch][metric]["worst"])
        elif metric == "speed":
            speeds_average.append(data[epoch][metric]["average"])
            speeds_top.append(data[epoch][metric]["top"])
            speeds_worst.append(data[epoch][metric]["worst"])




import matplotlib.pyplot as plt

#Fitness plot


plt.figure(figsize=(18, 3))

plt.subplot(131)
plt.title("Fitness")
plt.plot(epochs, fitness_average)
plt.plot(epochs, fitness_max)


plt.subplot(132)
plt.title("Size")
plt.plot(epochs, sizes_average)
plt.plot(epochs, sizes_top)
plt.subplot(133)
plt.title("Speed")
plt.plot(epochs, speeds_average)
plt.plot(epochs, speeds_top)


plt.show()




