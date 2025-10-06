# https://www.geeksforgeeks.org/deep-learning/transformers-pipeline/

from transformers import pipeline

# First, we create a fill-mask pipeline. This uses distilbert/distilroberta-base by default.
# Using this model, we predict words that are likely to fill in the <mask> token.
# We generate the top 20 predictions.
fill_mask_pipe = pipeline("fill-mask")
result = fill_mask_pipe("You played <mask>!", top_k=20) # As example, here we generate a word that describes how the player played.

# We then extract the complete sentences from the result.
sentences = [item['sequence'] for item in result]

# We create a sentiment-analysis pipeline. This uses distilbert/distilbert-base-oncased-finetuned-sst-2-english by default.
sentiment_pipeline = pipeline("sentiment-analysis")

# And get the sentiment score of every generated possible sentence in the top 20 predictions.
print("Scores:", sentiment_pipeline(sentences))

# From all the generated sentences, we split them up in two lists: negative sentiment and positive sentiment.
# This list looks as follows: [[sentence, POSITIVE/NEGATIVE, score], ...]
negative = []
positive = []
labels = [item['label'] for item in sentiment_pipeline(sentences)]
for i, sentence in enumerate(sentences):
    if labels[i] == 'NEGATIVE':
        negative.append([sentence, labels[i], sentiment_pipeline(sentences)[i]['score']])
    else:
        positive.append([sentence, labels[i], sentiment_pipeline(sentences)[i]['score']])

# Finally, we sort both lists by the score.      
# We use lambda x: x[2] to sort only by the score (which is at index 2 of both lists). 
# Sorting is from highest to lowest.
negative_sorted = sorted(negative, key=lambda x: x[2], reverse=True)
positive_sorted = sorted(positive, key=lambda x: x[2], reverse=True)

# The final lists are printed here.
print("Negative sorted:", negative_sorted)
print("Positive sorted:", positive_sorted)

# Next, we discuss how many of the top positive and top negative predictions we take. 
# This makes sure we can use different words to generate different sentences, whilst also making sure these different sentences have a similar sentiment!