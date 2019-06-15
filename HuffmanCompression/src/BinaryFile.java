import java.util.List;
import java.util.ArrayList;
import java.io.*;

public class BinaryFile {
	//writes a vector of strings to a file.  Assumes each character in each string is either a 0 or 1
	public static void writeToFile(List<Boolean> content, String output_file_name)
	{
		try
		{
			//open file in binary mode
			DataOutputStream output_file = new DataOutputStream(new FileOutputStream(output_file_name));
			
			//keeps track of all of the chars that we will write to file
			List<Integer> to_output = new ArrayList<>();
			
			//convert strings into vector of chars
			//the variable "buffer" here is used to store the binary strings
			//in a bit form that we want to write eventually, not as a "number"
			int buffer = 0;
			int buffer_counter = 0;
			
			//auto unwrapping from Boolean to boolean
			for(boolean item: content)
			{
				//bit shift all characters in the binary String 
				//into an int variable (32 bits long)
				if (buffer_counter == 32)
				{
					//buffer is full, save to the List
					to_output.add(buffer);
					buffer = 0;
					buffer_counter = 0;
				}
				
				//shift operators in Java returns int.
				//which is the reason we define "buffer" as an int 
				buffer = buffer << 1;
				if (item == true)
				{
					buffer = buffer | 1;					
				}
				
				buffer_counter ++;				
			}
			
			//account for any remaining buffer, and they are padded with 0s in the end.
			if (buffer_counter > 0)
			{
				while (buffer_counter < 32)
				{
					buffer = buffer << 1;
					buffer_counter ++;
				}
				to_output.add(buffer);
			}
			
			//finally, write to file
			//first entry indicates number of bits that are contained within the file
			//this is necessary because of the PADDING mentioned above, so we know where the coding ends.
			int num_bits = content.size();
			output_file.writeInt(num_bits); //writeInt() writes out the value as four bytes
			
			//now write the binary contents
			for (int item: to_output)
			{
				output_file.writeInt(item);
			}
			output_file.close();
		}
		catch (Exception e)
		{
			e.printStackTrace();
		}		
	}
	
	//reads a file written with writeToFilie into a list of bools
	public static List<Boolean> readFromFile(String input_file_name)
	{
		List<Boolean> result = new ArrayList<>();
		
		try
		{			
			DataInputStream input_file = new DataInputStream(new FileInputStream(input_file_name));
			
			//read the number of bits to read
			int bits_to_read = input_file.readInt();
			
			//now, start pulling in the int values one by one
			int single_int = 0;
			int bits_read = 0;
			
			for(int i = 0; i < bits_to_read; i += 32)
			{
				single_int = input_file.readInt();
				for (int j = 0; j < 32 && bits_read < bits_to_read; j ++)
				{
					//peel off one bit at a time
					//then shift the int to the left by one to get the next bit for the loop
					int bool_val = (single_int >> 31) & 1 ; //note you shift by 31, not 32 to get the leftmost bit; becasue you shift 0 to get the rightmost bit
					
					result.add(bool_val == 1 ? true : false);
					single_int = single_int << 1;
					bits_read++;
				}
			}
			
			//close file
			input_file.close();
		}
		catch(Exception e)
		{
			e.printStackTrace();
		}
		return result;
	}
}
