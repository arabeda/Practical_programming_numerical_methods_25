set title "Least-Squares Signal Smoothing"
set xlabel "Time"
set ylabel "Signal amplitude"
set grid
set key outside

plot \
"output_lambda_0.01.txt" using 1:4 with lines lw 2 title "Smoothed lambda=0.01", \
"output_lambda_0.10.txt" using 1:4 with lines lw 2 title "Smoothed lambda=0.10", \
"output_lambda_1.00.txt" using 1:4 with lines lw 2 title "Smoothed lambda=1.00", \
"output_lambda_10.00.txt" using 1:4 with lines lw 2 title "Smoothed lambda=10.00", \
"output_lambda_100.00.txt" using 1:4 with lines lw 2 title "Smoothed lambda=100.00", \
"output_lambda_0.01.txt" using 1:2 with lines lt 1 lw 3 title "Clean signal", \
"output_lambda_0.01.txt" using 1:3 with points pt 7 ps 0.5 title "Noisy signal"
