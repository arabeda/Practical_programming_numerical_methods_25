set terminal pngcairo size 800,600 enhanced font 'Arial,12'
set output 'fit.png'

set title "Higgs Boson Discovery Fit"
set xlabel "Energy E [GeV]"
set ylabel "Signal σ(E)"
set grid

plot \
  'data.txt' using 1:2:3 with yerrorbars title "Experimental Data" pt 7 ps 1 lc rgb "black", \
  'fit.txt' using 1:2 with lines lw 2 lc rgb "blue" title "Breit-Wigner Fit"