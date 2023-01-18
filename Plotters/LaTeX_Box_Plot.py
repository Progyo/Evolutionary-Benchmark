import json
import numpy as np

#The epochs to plot
epochs = [9] #,5, 25, 100


filePath= "../Data/run2-pop_4096_health_100_energy_50_foodCount_best_init_progressive.json"

#Import and prepare data

data = {}

with open(filePath,"r") as file:
    data = json.load(file)


#Template with parameters: Median, upper quartile, lower quartile
#Upper whisker, Lower whisker
#https://stackoverflow.com/questions/5466451/how-do-i-print-curly-brace-characters-in-a-string-while-using-format
plot_template = """\\addplot[
            boxplot prepared= {{
              median={},
              upper quartile={},
              lower quartile={},
              upper whisker={},
              lower whisker={}
            }},
            ] coordinates {{}};"""


#Template with parameters: Scale, ticks= e.g. {1,2,3}
#yticklabels = e.g. {Laufzeit 3, Laufzeit 2, Laufzeit 1} 
figure_Template = """\\begin{{figure}}[H]
    \\centering
    \\begin{{tikzpicture}}[scale={0}]
        \\begin{{axis}}
            [
            ytick={1},
            yticklabels={2},
            ]
            {3}
        \\end{{axis}}
    \\end{{tikzpicture}}
\\end{{figure}}"""



def GetPlot(median, upperQuart, lowerQuart, upperWhisk, lowerWhisk):

    return plot_template.format(median, upperQuart, lowerQuart, upperWhisk, lowerWhisk)


def GetFigure(scale, labels, plots):

    ytick = "{"
    ytickLabels = "{"
    for i in range(1,len(labels)+1):
        ytick += str(i)+","
        ytickLabels += labels[i-1]+"," 
    ytick = ytick[:-1]+"}"
    ytickLabels = ytickLabels[:-1] + "}"

    plot = ""
    for i in range(0,len(plots)):
        plot += plots[i]+"\n"

    return figure_Template.format(scale, ytick, ytickLabels, plot)


#print(GetPlot(0,0,0,0,0))
#print(GetFigure(1.0,["Epoch 1","Epoch 2","Epoch 3"]))


raw = {}
for epoch in epochs:

    try:
        assert str(epoch) in data

        raw[epoch] = {"blob-count": len(data[str(epoch)]["epoch"]),
                            "speed-values": [],
                            "size-values": [],
                            "fitness-values": [],
                            "health-values": [],
                            "energy-values": []}

        for blob in data[str(epoch)]["epoch"]:
            raw[epoch]["speed-values"].append(blob["speed"])
            raw[epoch]["size-values"].append(blob["size"])
            raw[epoch]["fitness-values"].append(blob["fitness"])
            raw[epoch]["health-values"].append(blob["health"])
            raw[epoch]["energy-values"].append(blob["energy"])


        try:
            assert len(raw[epoch]["speed-values"]) == raw[epoch]["blob-count"]
            assert len(raw[epoch]["size-values"]) == raw[epoch]["blob-count"]
            assert len(raw[epoch]["fitness-values"]) == raw[epoch]["blob-count"]
            assert len(raw[epoch]["health-values"]) == raw[epoch]["blob-count"]
            assert len(raw[epoch]["energy-values"]) == raw[epoch]["blob-count"]
        except AssertionError:
            print("Value inconsitency")
        
    except AssertionError:
        print("Epoch {} not found!".format(epoch))



for epoch in epochs:
    plots = []
    
    #Median
    medianSpeed = np.median(raw[epoch]["speed-values"])
    medianSize = np.median(raw[epoch]["size-values"])
    medianFitness = np.median(raw[epoch]["fitness-values"])
    medianHealth = np.median(raw[epoch]["health-values"])
    medianEnergy = np.median(raw[epoch]["energy-values"])

    #Quantiles

    upperQuantSpeed = np.quantile(raw[epoch]["speed-values"],[0.75])
    upperQuantSize = np.quantile(raw[epoch]["size-values"],[0.75])
    upperQuantFitness = np.quantile(raw[epoch]["fitness-values"],[0.75])
    upperQuantHealth = np.quantile(raw[epoch]["health-values"],[0.75])
    upperQuantEnergy = np.quantile(raw[epoch]["energy-values"],[0.75])


    lowerQuantSpeed = np.quantile(raw[epoch]["speed-values"],[0.25])
    lowerQuantSize = np.quantile(raw[epoch]["size-values"],[0.25])
    lowerQuantFitness = np.quantile(raw[epoch]["fitness-values"],[0.25])
    lowerQuantHealth = np.quantile(raw[epoch]["health-values"],[0.25])
    lowerQuantEnergy = np.quantile(raw[epoch]["energy-values"],[0.25])


    #Whiskers
    upperWhiskSpeed = np.quantile(raw[epoch]["speed-values"],[1.])
    upperWhiskSize = np.quantile(raw[epoch]["size-values"],[1.])
    upperWhiskFitness = np.quantile(raw[epoch]["fitness-values"],[1.])
    upperWhiskHealth = np.quantile(raw[epoch]["health-values"],[1.])
    upperWhiskEnergy = np.quantile(raw[epoch]["energy-values"],[1.])


    lowerWhiskSpeed = np.quantile(raw[epoch]["speed-values"],[0.])
    lowerWhiskSize = np.quantile(raw[epoch]["size-values"],[0.])
    lowerWhiskFitness = np.quantile(raw[epoch]["fitness-values"],[0.])
    lowerWhiskHealth = np.quantile(raw[epoch]["health-values"],[0.])
    lowerWhiskEnergy = np.quantile(raw[epoch]["energy-values"],[0.])



    plots.append(GetPlot(medianSpeed, upperQuantSpeed[0], lowerQuantSpeed[0], upperWhiskSpeed[0], lowerWhiskSpeed[0]))
    plots.append(GetPlot(medianSize, upperQuantSize[0], lowerQuantSize[0], upperWhiskSize[0], lowerWhiskSize[0]))
    plots.append(GetPlot(medianFitness, upperQuantFitness[0], lowerQuantFitness[0], upperWhiskFitness[0], lowerWhiskFitness[0]))
    #plots.append(GetPlot(medianHealth, upperQuantHealth[0], lowerQuantHealth[0], upperWhiskHealth[0], lowerWhiskHealth[0]))
    plots.append(GetPlot(medianEnergy, upperQuantEnergy[0], lowerQuantEnergy[0], upperWhiskEnergy[0], lowerWhiskEnergy[0]))


    print(GetFigure(1.0,["Speed", "Size", "Fitness", "Energy"],plots)) #, "Health", "Energy"
    
