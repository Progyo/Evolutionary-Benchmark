import json
import numpy as np

#The epochs to plot

printEvery = 5

toPrintTrait = ["fitness","size","speed"]

toPrint = "best"


filePath= "../Data/run2-pop_4096_health_100_energy_50_foodCount_best_init_progressive.json"

#Import and prepare data

data = {}

with open(filePath,"r") as file:
    data = json.load(file)


#Template - Form    x y label
#                   1 2 a 
#https://stackoverflow.com/questions/5466451/how-do-i-print-curly-brace-characters-in-a-string-while-using-format
plot_template = """\\addplot[scatter,
            scatter src=explicit symbolic]
        table[meta=label] {{% 
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




def GetPlot(x,y ,fill=True):
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


for trait in toPrintTrait:
    epochs = []
    values = []
    count = 1
    for epoch in data:

        
        if count % printEvery != 0 and count != 1:
            count+=1
            continue
        
        epochs.append(int(epoch))

        if toPrint == "best":
            if trait == "fitness":
                values.append(data[epoch][trait]["max"])
            else:
                values.append(data[epoch][trait]["top"])
        else:
            values.append(data[epoch][trait]["average"])

        count+=1
        
    print(GetFigure(1.0,[GetPlot(epochs, values)]))
