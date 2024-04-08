# PNGTuberPlusResizer
A utility for automatically resizing PNGTuber Plus models

Currently tested with PNGTuberPlus 1.4.5!

 After having had the experience of making a pngtuberplus model too large, thus having to manually tediously downscale all the animation anchor points/positions/animation amplitudes and such, and then seeing someone else had had a similar issue, I took some of the findings I've made while using this software and wrote a scaling utility for pngtuber plus models. (The whole "well screw doing this manually, the files are just written in json plaintext so I can ABSOLUTELY modify these pretty easily and help save time for others in the process" mindset)
 
I've noticed PNGTuberPlus chugs hard when it first loads up if you have overly large pngtuber models in there, especially models as complicated as the ones Sono rigs. And it's probably not necessary massive 5000px models for streaming purposes (I've found 2000px looks perfectly fine plus allows you to zoom in a reasonable amount when need be). Loading a model that large, I kid you not, consumed 7GB of ram on my machine (vs around 1.1GB with a 40% scale downscaled model). While the ram usage does go down overtime, I've noticed, that's still an INSANE amount for what's a bunch of images with physics transformers on them.

WHAT THIS ADJUSTS FOR RESIZING:
- Image file sizes (re-exports them all using the data in the save file)
- Embedded image file sizes
- Amplitudes (X/Y)
- Position coordinates
- Offset coordinates

How to use:
1. Select a model file.
2. Select a scale percentage
3. Select a folder to export the new file to.
4. Wait for the model to export. All the image files will be downscaled in a new folder, and all of the embedded images inside the save file itself will also be downsampled.
5. Open the model in PNGTuber Plus. You'll notice that nothing is animating; that's expected as I've noticed for physics stuff it doesn't like decimal values.
6. Wheelmouse through all of the sprites in edit mode (in edit mode, the wheelmouse selects the next sprite in the list of sprites), watch your avatar come alive, and then resave the file.

If this helps you out, awesome! I put the time in to try and save time for other people.
