#include spi.fs

$05 constant CMD_STATUS
$06 constant CMD_WR_ENABLE
$20 constant CMD_ERASE_4KB
$52 constant CMD_ERASE_32KB
$03 constant CMD_READ_DATA
$02 constant CMD_PAGE_PROG
$0B constant CMD_FAST_READ
$90 constant CMD_DEVICE_ID

: spi0-flash-status ( -- status )
  spi0-cs-low
  CMD_STATUS SPI0 spi-tx
  SPI0 spi-rx
  spi0-cs-high
;

: spi0-flash-wait-done
  begin
    spi0-flash-status
    1 and
    1-
  0<> until
;

: spi0-flash-read-id ( -- id )
  spi0-cs-low
  $90 SPI0 spi-tx
  0   SPI0 spi-tx
  0   SPI0 spi-tx
  0   SPI0 spi-tx
  SPI0 spi-rx 8 lshift
  SPI0 spi-rx or
  spi0-cs-high
;

: spi0-flash-read ( addr len -- bufaddr )
  spi0-cs-low

  CMD_READ_DATA          SPI0 spi-tx
  over 16 rshift $ff and SPI0 spi-tx
  over 8  rshift $ff and SPI0 spi-tx
  over           $ff and SPI0 spi-tx
  SPI0 over spi-rx-many

  spi0-cs-high
;

: spi0-write-enable ( -- )
  spi0-cs-low
  CMD_WR_ENABLE SPI0 spi-tx
  spi0-cs-high
;

: spi0-flash-write ( addr len bufaddr -- )
  >r  \ tuck away buffer address
  ( addr len bufaddr )
  spi0-write-enable

  spi0-cs-low

  CMD_PAGE_PROG          SPI0 spi-tx
  over 16 rshift $ff and SPI0 spi-tx
  over 8  rshift $ff and SPI0 spi-tx
  over           $ff and SPI0 spi-tx

  r>
  over 0 do
    dup i + c@ SPI0 spi-tx
  loop

  drop drop drop
  spi0-cs-high
  spi0-flash-wait-done
;

: spi0-flash-erase ( addr cmd -- )
  spi0-write-enable
  spi0-flash-wait-done
  spi0-cs-low
  ( cmd ) SPI0 spi-tx
  dup dup ( addr addr addr )
  16 rshift $ff and SPI0 spi-tx
  8  rshift $ff and SPI0 spi-tx
            $ff and SPI0 spi-tx
  spi0-cs-high
  spi0-flash-wait-done
;