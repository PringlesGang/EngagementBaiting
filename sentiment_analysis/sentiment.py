# https://huggingface.co/blog/sentiment-analysis-python

from transformers import pipeline
sentiment_pipeline = pipeline("sentiment-analysis")
data = ["Try again!", "You're getting better", "Practice makes perfect", "Take your time", "I believe in you", "You can do this"]


print(sentiment_pipeline(data))

# Try again!
# You're getting better
# Practice makes perfect
# Take your time
# I believe in you
# You can do this



# Negative feedback: [{'label': 'NEGATIVE', 'score': 0.9992058873176575}, {'label': 'NEGATIVE', 'score': 0.9814907312393188}, {'label': 'POSITIVE', 'score': 0.9922102689743042}, {'label': 'NEGATIVE', 'score': 0.9965564012527466}, {'label': 'POSITIVE', 'score': 0.9991829991340637}, {'label': 'NEGATIVE', 'score': 0.9957679510116577}, {'label': 'NEGATIVE', 'score': 0.9872899651527405}, {'label': 'POSITIVE', 'score': 0.894005298614502}, {'label': 'NEGATIVE', 'score': 0.999669075012207}, {'label': 'POSITIVE', 'score': 0.8481776714324951}, {'label': 'POSITIVE', 'score': 0.8618462681770325}, {'label': 'NEGATIVE', 'score': 0.9975351095199585}, {'label': 'POSITIVE', 'score': 0.9941278696060181}, {'label': 'NEGATIVE', 'score': 0.9839351773262024}]
# Positive feedback: [{'label': 'POSITIVE', 'score': 0.7419360280036926}, {'label': 'POSITIVE', 'score': 0.9995009899139404}, {'label': 'POSITIVE', 'score': 0.9998564720153809}, {'label': 'POSITIVE', 'score': 0.894005298614502}, {'label': 'POSITIVE', 'score': 0.9997929930686951}, {'label': 'POSITIVE', 'score': 0.9997914433479309}]