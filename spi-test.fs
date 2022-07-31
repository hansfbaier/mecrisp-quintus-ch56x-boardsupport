
#require spi.fs
#require dump.fs

SPI0 spi-master-init
here $100 allot constant buffer
: wdata $ff 0 do i . space $ff i - buffer i + c! ." => " buffer i + c@ . space space loop ;
wdata
$500 $ff buffer spi0-flash-write
$500 $ff spi0-flash-read dup $100 dump