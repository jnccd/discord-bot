@echo off
cd Commands
cd Latex
pdflatex -jobname=output input >nul 2>&1
magick.exe -density 300 output.pdf output.png >nul 2>&1
