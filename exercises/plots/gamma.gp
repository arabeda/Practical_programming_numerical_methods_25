set terminal svg background "white"
set output "gamma.svg"
set xlabel "x"
set ylabel "gamma(x)"
set yrange [0:40000]
plot "gamma.txt" with lines title "gamma(x)"