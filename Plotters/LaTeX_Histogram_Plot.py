import json
import numpy as np

#The epochs to plot

epochs = [9] #5, 25, 100 np.arange(1,25)#

bins = 15



filePath= "../Data/run2-pop_4096_health_100_energy_50_foodCount_best_init_progressive.json"

#Import and prepare data

data = {}

with open(filePath,"r") as file:
    data = json.load(file)



##Template - Form:    (x, y) 
plot_template = """\\addplot+[ybar interval,mark=no] plot coordinates {{
            {}
            }};"""


#Template with parameters: Scale, min, max, plots
figure_Template = """\\begin{{figure}}[H]
    \\centering
    \\begin{{tikzpicture}}[scale={}]
        \\begin{{axis}}[
            ymin={}, ymax={},
            minor y tick num = 2,
            area style,
            ]
            {}
        \\end{{axis}}
    \\end{{tikzpicture}}
\\end{{figure}}"""





def GetPlot(x,y):
    try:
        assert len(x) == len(y)
        out = ""
        for i in range(0,len(x)):
            out+="({}, {})\n".format(x[i],y[i])
        return plot_template.format(out)
    except:
        print("X and Y not the same length!")

    
    return ""
    


def GetFigure(scale,minVal,maxVal,plots):

    plot = ""
    for i in range(0,len(plots)):
        plot += plots[i]+"\n"

    return figure_Template.format(scale,minVal,maxVal,plot)


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

    

    sizeBins = (max(raw[epoch]["size-values"])-3)

    #https://numpy.org/doc/stable/reference/generated/numpy.histogram.html
    speedHisto, _ = np.histogram(raw[epoch]["speed-values"],bins=bins)
    sizeHisto, _ = np.histogram(raw[epoch]["size-values"],bins=sizeBins)
    fitnessHisto, _ = np.histogram(raw[epoch]["fitness-values"],bins=bins)
    healthHisto, _ = np.histogram(raw[epoch]["health-values"],bins=bins)
    energyHisto, _ = np.histogram(raw[epoch]["energy-values"],bins=bins)


    plots.append(GetPlot(np.round(np.arange(3,sizeBins+3),2),sizeHisto))

    print(GetFigure(1.0,0,max(sizeHisto) * 5 / 4, plots))
