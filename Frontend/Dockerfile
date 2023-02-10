FROM node:18.14.0

# Change working directory to newly created app dir
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Copy package.json and package-lock.json to /app dir
COPY package.json .

# Install dependencies
RUN npm install

# Copy the source code to /app dir
COPY . .

# Expose port 3000 on the container
EXPOSE 3000

# Run the app
CMD ["npm", "run", "dev"]
