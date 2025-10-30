#!/bin/bash
# VISO - Virtual ISO Booter
# Writes ISO to swap, creates virtual USB device

set -e

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

echo -e "${GREEN}╔═══════════════════════════════════════╗${NC}"
echo -e "${GREEN}║  VISO - Virtual ISO Boot Creator     ║${NC}"
echo -e "${GREEN}║  Smarter than Rufus, safer than dd   ║${NC}"
echo -e "${GREEN}╚═══════════════════════════════════════╝${NC}"
echo ""

# Check root
if [[ $EUID -ne 0 ]]; then
   echo -e "${RED}Must run as root (sudo)${NC}"
   exit 1
fi

# Get ISO file
if [ -z "$1" ]; then
    echo "Usage: sudo $0 <iso-file> [swap-partition]"
    echo "Example: sudo $0 ubuntu.iso /dev/sda2"
    echo ""
    echo "Available swap partitions:"
    swapon --show
    exit 1
fi

ISO_FILE="$1"
SWAP_PART="${2:-auto}"

# Validate ISO
if [ ! -f "$ISO_FILE" ]; then
    echo -e "${RED}ISO file not found: $ISO_FILE${NC}"
    exit 1
fi

ISO_SIZE=$(stat -f%z "$ISO_FILE" 2>/dev/null || stat -c%s "$ISO_FILE")
ISO_SIZE_MB=$((ISO_SIZE / 1024 / 1024))

echo -e "${YELLOW}ISO: $ISO_FILE (${ISO_SIZE_MB}MB)${NC}"

# Find swap if auto
if [ "$SWAP_PART" = "auto" ]; then
    SWAP_PART=$(swapon --show --noheadings | head -1 | awk '{print $1}')
    if [ -z "$SWAP_PART" ]; then
        echo -e "${RED}No active swap found!${NC}"
        exit 1
    fi
    echo -e "${YELLOW}Auto-detected swap: $SWAP_PART${NC}"
fi

# Validate swap partition exists
if [ ! -b "$SWAP_PART" ]; then
    echo -e "${RED}Swap partition not found: $SWAP_PART${NC}"
    exit 1
fi

SWAP_SIZE=$(lsblk -b -n -o SIZE "$SWAP_PART")
SWAP_SIZE_MB=$((SWAP_SIZE / 1024 / 1024))

echo -e "${YELLOW}Swap: $SWAP_PART (${SWAP_SIZE_MB}MB)${NC}"

if [ $ISO_SIZE -gt $SWAP_SIZE ]; then
    echo -e "${RED}ISO too large for swap! Need ${ISO_SIZE_MB}MB, have ${SWAP_SIZE_MB}MB${NC}"
    exit 1
fi

# Warning
echo ""
echo -e "${RED}⚠️  WARNING ⚠️${NC}"
echo "This will:"
echo "1. Disable swap temporarily"
echo "2. Write ISO to $SWAP_PART"
echo "3. Create virtual USB device"
echo ""
read -p "Continue? (yes/no): " confirm

if [ "$confirm" != "yes" ]; then
    echo "Aborted."
    exit 0
fi

# Disable swap
echo -e "\n${GREEN}[1/4] Disabling swap...${NC}"
swapoff "$SWAP_PART"

# Write ISO
echo -e "${GREEN}[2/4] Writing ISO to swap partition...${NC}"
dd if="$ISO_FILE" of="$SWAP_PART" bs=4M status=progress oflag=sync

# Create loop device
echo -e "${GREEN}[3/4] Creating virtual USB device...${NC}"
LOOP_DEV=$(losetup -f)
losetup "$LOOP_DEV" "$SWAP_PART"

# Create mount point
MOUNT_POINT="/mnt/viso-$(basename "$ISO_FILE" .iso)"
mkdir -p "$MOUNT_POINT"

# Try to mount
echo -e "${GREEN}[4/4] Mounting virtual USB...${NC}"
if mount -o ro "$LOOP_DEV" "$MOUNT_POINT" 2>/dev/null; then
    echo -e "\n${GREEN}✓ SUCCESS!${NC}"
    echo ""
    echo "Virtual USB created at: $MOUNT_POINT"
    echo "Loop device: $LOOP_DEV"
    echo ""
    echo "To boot from this in QEMU:"
    echo "  qemu-system-x86_64 -m 2048 -boot d -drive file=$SWAP_PART,format=raw"
    echo ""
    echo "To unmount and restore swap:"
    echo "  sudo umount $MOUNT_POINT"
    echo "  sudo losetup -d $LOOP_DEV"
    echo "  sudo swapon $SWAP_PART"
else
    echo -e "${YELLOW}Note: ISO written but couldn't mount (might not be mountable, still bootable!)${NC}"
    echo ""
    echo "To boot in QEMU:"
    echo "  qemu-system-x86_64 -m 2048 -boot d -drive file=$SWAP_PART,format=raw"
    echo ""
    echo "To restore swap:"
    echo "  sudo losetup -d $LOOP_DEV"
    echo "  sudo swapon $SWAP_PART"
fi

echo ""
echo -e "${GREEN}Virtual ISO boot device ready!${NC}"
