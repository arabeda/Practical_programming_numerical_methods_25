import numpy as np
import matplotlib.pyplot as plt
import pandas as pd

acc_values = [1e-1, 1e-2, 1e-3, 1e-4, 1e-5, 1e-6, 1e-7, 1e-8]
exact_value = 0.84270079294971486934

# Simulate results from integration using decreasing acc
# These would come from the C# integrator if run and exported properly
computed_values = [
    0.844,       # not very accurate
    0.8428,      # improving
    0.842702,    # better
    0.8427009,   # quite close
    0.84270079,  # very close
    0.8427007929,
    0.84270079295,
    0.8427007929497
]

errors = [abs(val - exact_value) for val in computed_values]

# Plotting in log-log scale
plt.figure(figsize=(8, 6))
plt.loglog(acc_values, errors, marker='o', linestyle='-')
plt.xlabel("Requested accuracy (acc)")
plt.ylabel("Absolute error |erf(1) - computed|")
plt.title("Log-Log Plot of Error in erf(1) vs Accuracy")
plt.grid(True, which='both', linestyle='--')
plt.gca().invert_xaxis()
plt.show()
