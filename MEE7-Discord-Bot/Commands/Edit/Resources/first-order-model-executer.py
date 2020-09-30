import imageio
import os 
import sys
import numpy as np
import matplotlib.pyplot as plt
import matplotlib.animation as animation
from skimage.transform import resize
from IPython.display import HTML
import warnings
warnings.filterwarnings("ignore")
sys.path.append('../first-order-model')

print(os.getcwd())
try:
    os.chdir(r"Commands\Edit\first-order-model")
except:
    print("No chdir")
print(os.getcwd())

source_image = imageio.imread('./content/gdrive/My Drive/first-order-motion-model/02.png')
driving_video = imageio.mimread('./content/gdrive/My Drive/first-order-motion-model/04.mp4')

#Resize image and video to 256x256

source_image = resize(source_image, (256, 256))[..., :3]
driving_video = [resize(frame, (256, 256))[..., :3] for frame in driving_video]

def display(source, driving, generated=None):
    fig = plt.figure(figsize=(8 + 4 * (generated is not None), 6))

    ims = []
    for i in range(len(driving)):
        cols = [source]
        cols.append(driving[i])
        if generated is not None:
            cols.append(generated[i])
        im = plt.imshow(np.concatenate(cols, axis=1), animated=True)
        plt.axis('off')
        ims.append([im])

    ani = animation.ArtistAnimation(fig, ims, interval=50, repeat_delay=1000)
    plt.close()
    return ani

#HTML(display(source_image, driving_video).to_html5_video())

print('check')

from demo import load_checkpoints
generator, kp_detector = load_checkpoints(config_path='config/vox-256.yaml', 
                            checkpoint_path='./content/gdrive/My Drive/first-order-motion-model/vox-cpk.pth.tar')

print('pred')

from demo import make_animation
from skimage import img_as_ubyte

predictions = make_animation(source_image, driving_video, generator, kp_detector, relative=True)

print('save')

#save resulting video
imageio.mimsave('./content/generated.mp4', [img_as_ubyte(frame) for frame in predictions])

print('ffmpeg')

os.system('ffmpeg -itsscale 0.334688995215311 -i ./content/generated.mp4 -y -c copy ./content/output.mp4')
os.system('ffmpeg -i ./content/output.mp4 -i ./content/audio.mp3 -y -shortest ./content/final.mp4')