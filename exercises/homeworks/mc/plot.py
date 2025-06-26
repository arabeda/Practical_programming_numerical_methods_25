import matplotlib.pyplot as plt
import numpy as np


N_values = [10**i for i in range(9)]


mc_errors = [3.1, 1.1, 0.3, 0.09, 0.031, 0.011, 0.004, 0.0013, 0.0004]     
qmc_errors = [1.2, 0.4, 0.1, 0.025, 0.008, 0.0025, 0.0009, 0.0003, 0.0001]

plt.figure(figsize=(8,6))
plt.loglog(N_values, mc_errors, 'o-', label='Monte Carlo (MC)')
plt.loglog(N_values, qmc_errors, 's-', label='Quasi-Monte Carlo (QMC)')


ref = [1 / np.sqrt(N) for N in N_values]
plt.loglog(N_values, ref, '--', label='~1/√N', color='gray')

plt.xlabel('Number of samples N')
plt.ylabel('Absolute error')
plt.title('MC vs QMC: Error vs Number of Samples')
plt.grid(True, which='both', linestyle='--')
plt.legend()
plt.tight_layout()
plt.show()
