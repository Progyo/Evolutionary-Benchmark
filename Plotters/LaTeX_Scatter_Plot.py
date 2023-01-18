import json
import numpy as np

#The epochs to plot
epochs = [100] #,5, 25, 100


filePath= "../Data/run2-pop_4096_health_100_energy_50_foodCount_best_init_progressive.json"

#The trait on the X axis
traitX = "speed"

#The trait on the Y axis
traitY = "size"

#The trait to use to colour
traitColour = "fitness"

#Import and prepare data

data = {}

with open(filePath,"r") as file:
    data = json.load(file)


#Template with parameters: Median, upper quartile, lower quartile
#Upper whisker, Lower whisker
#https://stackoverflow.com/questions/5466451/how-do-i-print-curly-brace-characters-in-a-string-while-using-format
plot_template = """\\addplot[scatter,only marks,
                scatter src=explicit symbolic]
            table[meta=label] {{ 
            x y label
            {}
            }};"""


#Template with parameters: Scale 
figure_Template = """\\begin{{figure}}[H]
    \\centering
    \\begin{{tikzpicture}}[scale={}]
        \\begin{{axis}}[
        scatter/classes={{
            a={{mark=o,draw=black}} }}]
        {}
        \\end{{axis}}
    \\end{{tikzpicture}}
\\end{{figure}}"""


def GetPlot(x,y,fill=True):
    try:
        assert len(x) == len(y)
        out = ""
        for i in range(0,len(x)):

            out+="{} {} {}\n".format(x[i],y[i],  "b" if fill else "a")
            
        return plot_template.format(out)
    except:
        print("X and Y not the same length!")

    
    return ""
def GetFigure(scale,plots):

    plot = ""
    for i in range(0,len(plots)):
        plot += plots[i]+"\n"

    return figure_Template.format(scale,plot)


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
            #print(xVal)
            #finalX.append(xVal)
            #finalY.append(0.0)
            for yVal in coords[xVal]:
                finalX.append(xVal)
                finalY.append(yVal)        

        print(len(finalX))
        print(len(finalY))
        print(count)
        #print(list(coords.keys())[20])
        #print(coords[list(coords.keys())[20]][0])
       
    except:
        print("X and Y not the same length!")

    


    
    return finalX, finalY


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

    xTrait = "{}-values".format(traitX)
    yTrait = "{}-values".format(traitY)
    colourTrait = "{}-values".format(traitColour)
    

    x,y = CleanUp(raw[epoch][xTrait],raw[epoch][yTrait])

    print(GetFigure(1.0,[GetPlot(x,y)]))

    
