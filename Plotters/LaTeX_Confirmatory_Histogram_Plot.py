import json
import numpy as np


#This histogram should generate the distribution of the fitness scores


bins = 15


filePath= "../Data/comparison-pop_4096_health_100_energy_50_speed10size3_vspeed5size5__progressive.json"

#The trait on the X axis
trait = "size"

sizeA = 3
sizeB = 5




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


def CleanUp(x,y):

    coords = {}

    finalX = []
    finalY = []

    count = 0

    try:
        
        assert len(x) == len(y)
        for i in range(0,len(x)):
            
            if not (x[i] in coords):
                coords[x[i]] = []
            if not (y[i] in coords[x[i]]):
                coords[x[i]].append(y[i])
                count += 1

        for xVal in coords.keys():

            for yVal in coords[xVal]:
                finalX.append(xVal)
                finalY.append(yVal)        

        #print(len(finalX))
        #print(len(finalY))
        #print(count)

       
    except:
        print("Clean up: X and Y not the same length!")

    


    
    return finalX, finalY


raw = {}
for epoch in data:

    try:
        assert str(epoch) in data

        raw[epoch] = {}
        

        raw[epoch]["A"] = {"blob-count": 0,
                            "speed-values": [],
                            "size-values": [],
                            "fitness-values": [],
                            "health-values": [],
                            "energy-values": []}

        raw[epoch]["B"] = {"blob-count": len(data[str(epoch)]["epoch"]),
                            "speed-values": [],
                            "size-values": [],
                            "fitness-values": [],
                            "health-values": [],
                            "energy-values": []}

        for blob in data[str(epoch)]["epoch"]:

            if trait == "size":
                if blob["size"] == sizeA:
                    raw[epoch]["A"]["speed-values"].append(blob["speed"])
                    raw[epoch]["A"]["size-values"].append(blob["size"])
                    raw[epoch]["A"]["fitness-values"].append(blob["fitness"])
                    raw[epoch]["A"]["health-values"].append(blob["health"])
                    raw[epoch]["A"]["energy-values"].append(blob["energy"])
                    raw[epoch]["A"]["blob-count"] += 1
                elif blob["size"] == sizeB:
                    raw[epoch]["B"]["speed-values"].append(blob["speed"])
                    raw[epoch]["B"]["size-values"].append(blob["size"])
                    raw[epoch]["B"]["fitness-values"].append(blob["fitness"])
                    raw[epoch]["B"]["health-values"].append(blob["health"])
                    raw[epoch]["B"]["energy-values"].append(blob["energy"])
                    raw[epoch]["B"]["blob-count"] += 1

                else:
                    print("Not matching size found!!")



        raw[epoch]["A"]["average-fitness"] = np.mean(raw[epoch]["A"]["fitness-values"])
        raw[epoch]["B"]["average-fitness"] = np.mean(raw[epoch]["B"]["fitness-values"]) 
        
    except AssertionError:
        print("Epoch {} not found!".format(epoch))




plots = []
valuesX = []
valuesY = []
for epoch in data:
    valuesX.append(raw[epoch]["A"]["average-fitness"])
    valuesY.append(raw[epoch]["B"]["average-fitness"])


x,y = CleanUp(valuesX,valuesY)





plots = []

xHisto, xLabel = np.histogram(x,bins=100)
yHisto, yLabel = np.histogram(y,bins=100)


plots.append(GetPlot(np.round(xLabel[:-1],2),xHisto))

print(GetFigure(1.0,0,max(xHisto) * 5 / 4, plots))
