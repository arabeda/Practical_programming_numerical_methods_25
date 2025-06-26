# Least-Squares Signal Smoothing in C#

This project implements **signal smoothing using the ordinary least-squares method with second-derivative regularization**, based on numerical techniques described in a scientific text on numerical methods.

---

## Overview

Noisy signals are common in real-world data. This project demonstrates how to smooth such noisy signals by minimizing a cost function combining fidelity to the original data and a penalty on the roughness of the smoothed signal, measured by its second derivative.

The smoothing problem is formulated as:

$$
\min_x \|x - y\|^2 + \lambda \|D x\|^2
$$

where:  
- \( y \) is the noisy signal,  
- \( x \) is the smoothed signal to find,  
- \( D \) is a discrete second-difference operator approximating the second derivative,  
- \( \lambda > 0 \) is a smoothing parameter controlling the trade-off between fidelity and smoothness.

---

## Features

- Generates example signals (sum of sine and cosine waves) with added Gaussian noise.  
- Implements least-squares smoothing by solving a linear system using Gaussian elimination.  
- Supports multiple smoothing parameters (\(\lambda\)) for comparison.  
- Outputs tab-separated data files suitable for plotting in Gnuplot or other tools.  
- Includes a sample Gnuplot script for visualizing clean, noisy, and smoothed signals.

---

## Usage

1. **Compile and run the C# program** (`Program.cs`). It will generate several output files like `output_lambda_0.01.txt`, `output_lambda_0.10.txt`, etc.  
2. **Plot the results** using the included Gnuplot script (`plot.gp`) or your preferred plotting software.  
3. Experiment with different lambda values in the source code to see how smoothing behavior changes.

---

## Requirements

- .NET SDK (tested with .NET 6 and later).  
- Gnuplot for plotting (optional, for visualization).  

---

## How to Run

```bash
dotnet run
