#!/bin/bash
# Cleanup and restore swap

if [[ $EUID -ne 0 ]]; then
   echo "Must run as root (sudo)"
   exit 1
fi

# Find viso mounts
for mount in /mnt/viso-*; do
    if mountpoint -q "$mount" 2>/dev/null; then
        echo "Unmounting $mount..."
        umount "$mount"
        rmdir "$mount"
    fi
done

# Detach loop devices on swap
for loop in $(losetup -a | grep swap | cut -d: -f1); do
    echo "Detaching $loop..."
    losetup -d "$loop"
done

# Re-enable all swap
echo "Re-enabling swap..."
swapon -a

echo "✓ Cleanup complete - swap restored!"
