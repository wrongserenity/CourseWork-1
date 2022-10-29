# Graduate qualified bachelor work

### Application of artificial intelligence methods in computer games

Saint-Petersburg State University <br>
Department of Programming Technology

Within an education program 01.03.02: <br> 
Applied Mathematics and Informatics

> Supervisor: <br>
> Candidate of Engineering <br>
> I. S. Blekanov <br>

> Supervisor: <br>
> Senior Lecturer <br>
> A. B. Stuchenkov <br>

> Reviewer: <br>
> Ph.D., Associate Professor <br>
> A. M. Kovshov <br>

## Target:
Exploring the possibilities of using machine learning methods to model the behavior of game artificial intelligence (GAI).

## Project architecture
![Image alt](https://github.com/wrongserenity/CourseWork-1/raw/main/Assets/Progress/project_architecture.png)

## Neural network architecture
![Image alt](https://github.com/wrongserenity/CourseWork-1/raw/main/Assets/Progress/nn_architecture.png)

## Learning
On the graphs you can see the dependence of the lifetime on the generation.
### First attempt
![Image alt](https://github.com/wrongserenity/CourseWork-1/raw/main/Assets/Progress/learn_v1.png)
### Second attempt
![Image alt](https://github.com/wrongserenity/CourseWork-1/raw/main/Assets/Progress/learn_v2.png)
### Learn comparison
![Image alt](https://github.com/wrongserenity/CourseWork-1/raw/main/Assets/Progress/learn_comparison.png)

## Hyperparameter optimization
![Image alt](https://github.com/wrongserenity/CourseWork-1/raw/main/Assets/Progress/optimization_results.png)
- Values for hyperparameters, the influence of which is difficult to assess unambiguously, are highlighted in gray.
- The line average denotes the average value of training, regardless of all other parameters. Rows 10 worst and 10 best show which hyperparameters made it into the list of 10 worst and best training settings.
- So, for the values of the rows average and 10 best, the largest values are searched, and for 10 worst, the smallest ones.

## Learning process visualization
![Image alt](https://github.com/wrongserenity/CourseWork-1/raw/main/Assets/Progress/learn_process_layers.png)
![Image alt](https://github.com/wrongserenity/CourseWork-1/raw/main/Assets/Progress/learning_process.gif)

## References
- Yunqi Zhao, Igor Borovikov, Jason Rupert, Caedmon Somers, Ahmad Bierami. **On Multi-Agent Learning in Team Sports Games**. ICML Workshop on Imitation, Intent, and Interaction (I3), 2019.
- Samuel, A. IBM Journal of Research and Development. **Some studies in machine learning using the game of checkers**. 10th Computer Science and Electronic Engineering (CEEC), 2018.
- C. Arzate Cruz and J. A. Ramirez Uresti, **Hrlb2: A reinforcement learning based framework for believable bots**. Applied Sciences, 2018.
- Adarsh Sehgal, Hung Manh La, Sushil J. Louis, Hai Nguyen. **Deep Reinforcement Learning using Genetic Algorithm for Parameter Optimization**. Third IEEE International Conference on Robotic Computing (IRC), 2019.
- Review article:
Boming Xia, Xiaozhen Ye, Adnan O.M.Abuassba. **Recent Research on AI in Games**. International Wireless Communications and Mobile Computing (IWCMC), 2020.
